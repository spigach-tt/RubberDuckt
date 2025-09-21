# Rubber Duck(t)
A 3D modeling plugin that provides early-stage specialist guidelines tailored for small to medium architecture firms, bridging the gap before consultants are engaged.


<img width="852" height="164" alt="image" src="https://github.com/user-attachments/assets/799f6c0c-2c67-4726-83df-25dec66f0eb4" />

This project was developed at the 2025 AECtech Boston hackathon.

## Hackathon Team
- Anthony F. Samaha | Perkins&Will
- Emre C. Baykurt | Boston University
- Ethnie Xu | Hart Howerton
- Ioannis Kopsacheilis | Enemco
- Nicolas Martinez | /slantis
- Qiayi Huang | Boston University
- Sergey Pigach | CORE studio at Thornton Tomasetti
- Victor W. Barbosa | Schmidt Hammer Lassen

## Software Architecture

<img width="854" height="424" alt="image" src="https://github.com/user-attachments/assets/f073e8de-bed3-4451-87d9-0849b3e8b951" />

## Prerequisites
1. Rhino 8 or newer
2. Visual Studio 2022
3. RhinoMCP ([Install here][https://github.com/jingcheng-chen/rhinomcp])

## Installation Instructions
1. Open the solution in Visual Studio
2. Build as Debug or Release
3. Find the `.rhp` file in the bin folder and drag it into Rhino

## Usage Instructions
1. Set up your Rhino file with the following layers:
  - MEP
  - Serviced
  - Not Serviced
2. In Rhino's Properties panel, assign names to room volumes based on their program
3. Run `DuckParse` to parse your Rhino scene and save the data as a JSON file to disk
