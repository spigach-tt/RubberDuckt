import { z } from "zod";

export const point3dSchema = z.object({
	x: z.number(),
	y: z.number(),
	z: z.number(),
});

export const polylineSchema = z.object({
	points: z.array(point3dSchema),
});

export const roomSchema = z.object({
	id: z.string(),
	name: z.string(),
	area: z.number().min(0),
	height: z.number().min(0),
	geometry: polylineSchema,
});

export const levelSchema = z.object({
	name: z.string(),
	rooms: z.array(roomSchema).min(1),
});

// Duct routing schemas
export const branchSchema = z.object({
	roomId: z.string().describe("The ID of the room this branch serves"),
	points: z
		.array(point3dSchema)
		.min(2)
		.describe("Ordered coordinate points defining the branch polyline path"),
});

export const ductRoutingSchema = z.object({
	mainTrunk: z.object({
		points: z
			.array(point3dSchema)
			.min(2)
			.describe(
				"Ordered coordinate points defining the main trunk polyline path",
			),
	}),
	branches: z
		.array(branchSchema)
		.min(1)
		.describe("Array of branch connections, one for each room"),
});
