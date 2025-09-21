using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using RubberDuck.IO;

public static class DuckGeomUtils
{
    public static bool TryBuildDtoFromRhinoObject(RhinoDoc doc, RhinoObject obj, out MepDto dto)
    {
        dto = null;
        if (obj?.Geometry == null) return false;

        // Normalize to Brep first (works for Brep, Extrusion, and most Surfaces)
        Brep brep = null;

        switch (obj.Geometry)
        {
            case Brep asBrep:
                brep = asBrep;
                break;

            case Extrusion ex:
                // Convert extrusion to Brep; 'true' splits at kinks and is typically fine
                brep = ex.ToBrep(true) ?? ex.ToBrep(false);
                break;

            case Surface srf:
                // Convert surface to Brep if needed
                brep = srf.ToBrep();
                break;
        }

        if (brep == null) return false;

        // Compute height from world-Z bbox (generic and robust)
        var bbox = brep.GetBoundingBox(true);
        var height = bbox.Max.Z - bbox.Min.Z;

        // Identify bottom planar face(s): face bbox Min.Z ≈ brep bbox Min.Z
        var tol = doc?.ModelAbsoluteTolerance ?? 0.001;
        double zMin = bbox.Min.Z;
        BrepFace bottomFace = null;
        double bottomFaceArea = 0.0;

        foreach (var face in brep.Faces)
        {
            // Require planarity for "bottom"
            if (!face.IsPlanar(tol)) continue;

            var fb = face.GetBoundingBox(true);
            if (Math.Abs(fb.Min.Z - zMin) <= tol + 1e-6)
            {
                var amp = AreaMassProperties.Compute(face);
                if (amp != null && amp.Area > bottomFaceArea)
                {
                    bottomFaceArea = amp.Area;
                    bottomFace = face;
                }
            }
        }

        // Fallback: if we didn't find a bottom planar face, use the largest planar face
        if (bottomFace == null)
        {
            foreach (var face in brep.Faces)
            {
                if (!face.IsPlanar(tol)) continue;
                var amp = AreaMassProperties.Compute(face);
                if (amp != null && amp.Area > bottomFaceArea)
                {
                    bottomFaceArea = amp.Area;
                    bottomFace = face;
                }
            }
        }

        // Build the dto
        var mep = new MepDto
        {
            Id = obj.Id.ToString(),
            Height = height,
            Area = bottomFaceArea,
            Geometry = ExtractDuckGeometryFromBottomFace(bottomFace, tol)
        };

        dto = mep;
        return true;
    }

    /// <summary>
    /// Returns ordered boundary points from the bottom face.
    /// If multiple outer boundaries exist (uncommon), concatenates them one after another.
    /// </summary>
    private static DuckGeometry ExtractDuckGeometryFromBottomFace(BrepFace face, double tol)
    {
        var geom = new DuckGeometry();

        if (face == null)
            return geom;

        // Collect OUTER loops first; inner loops are holes—include only if you want hole contours too.
        var loops = face.Loops.Where(l => l.LoopType == BrepLoopType.Outer).ToList();
        if (loops.Count == 0)
            loops = face.Loops.ToList(); // fallback

        foreach (var loop in loops)
        {
            // Make a single boundary curve
            var loopCrv = loop.To3dCurve();
            if (loopCrv == null) continue;

            // Prefer a polyline approximation for ordered points
            // (tolerances: angle/radius based on model tolerance)
            var polyCrv = loopCrv.ToPolyline(
                angleTolerance: RhinoMath.ToRadians(1.0),
                tolerance: tol,
                minimumLength: tol,
                maximumLength: 0.0);

            if (polyCrv != null && polyCrv.TryGetPolyline(out Polyline poly))
            {
                // Avoid duplicating the closing point
                int n = poly.Count;
                int end = (n > 1 && poly[0].DistanceTo(poly[n - 1]) <= tol) ? n - 1 : n;

                for (int i = 0; i < end; i++)
                {
                    var p = poly[i];
                    geom.Points.Add(new DuckPoint(p.X, p.Y, p.Z));
                }
            }
            else
            {
                // As a fallback, sample the curve by length
                int divs = Math.Max(16, (int)Math.Ceiling(loopCrv.GetLength() / Math.Max(tol, 0.1)));
                if (divs < 2) divs = 2;

                var tParams = loopCrv.DivideByCount(divs, true);
                if (tParams != null && tParams.Length > 0)
                {
                    foreach (var t in tParams)
                    {
                        var p = loopCrv.PointAt(t);
                        geom.Points.Add(new DuckPoint(p.X, p.Y, p.Z));
                    }
                }
            }
        }

        return geom;
    }
}
