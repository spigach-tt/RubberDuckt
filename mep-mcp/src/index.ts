import { createOpenAI } from "@ai-sdk/openai";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { McpAgent } from "agents/mcp";
import { generateObject } from "ai";
import { ductRoutingSchema, levelSchema } from "./domain";
import { ductRoutingPrompt } from "./prompts";

export interface Env {
	OPENAI_API_KEY: string;
}

// Define our MCP agent with tools
export class MyMCP extends McpAgent<Env> {
	server = new McpServer({
		name: "Mechanical Engineering Assistant",
		version: "1.0.0",
	});

	async init() {
		this.server.tool(
			"requiredShaftArea",
			"Calculate the required shaft area for a level of a building. This is a function that takes a level object and returns a shaft area object.",
			{ level: levelSchema },
			async ({ level }) => {
				const floorArea = level.rooms.reduce((sum, room) => sum + room.area, 0);

				if (floorArea <= 100) {
					const shaftArea = floorArea * 0.1;
					const numberOfShafts = 1;
					return {
						content: [
							{
								type: "text",
								text: `The required shaft area for level ${level.name} is ${shaftArea}, distributed across ${numberOfShafts} shafts.`,
							},
							{ type: "text", text: String(shaftArea) },
							{ type: "text", text: String(numberOfShafts) },
						],
					};
				} else if (floorArea <= 500) {
					const shaftArea = (0.1 * floorArea) / 2;
					const numberOfShafts = 2;
					return {
						content: [
							{
								type: "text",
								text: `The required shaft area for level ${level.name} is ${shaftArea}, distributed across ${numberOfShafts} shafts.`,
							},
							{ type: "text", text: String(shaftArea) },
							{ type: "text", text: String(numberOfShafts) },
						],
					};
				} else if (floorArea <= 2000) {
					const numberOfShafts = 3;
					const shaftArea = (0.1 * floorArea) / 3;
					return {
						content: [
							{
								type: "text",
								text: `The required shaft area for level ${level.name} is ${shaftArea}, distributed across ${numberOfShafts} shafts.`,
							},
							{ type: "text", text: String(shaftArea) },
							{ type: "text", text: String(numberOfShafts) },
						],
					};
				} else {
					return {
						isError: true,
						content: [
							{
								type: "text",
								text: `Error: I can't calculate shaft area for floors above 2000.`,
							},
						],
					};
				}
			},
		);

		this.server.tool(
			"ductRouting",
			"Route the ducts for a level of a building. This is a function that takes a level object and returns a duct routing object.",
			{ level: levelSchema },
			async ({ level }) => {
				const openai = createOpenAI({
					apiKey: this.env.OPENAI_API_KEY,
				});

				console.log("Generating ducts for level", level.name);
				const ducts = await generateObject({
					model: openai("gpt-5-nano"),
					providerOptions: {
						openai: {
							apiKey: this.env.OPENAI_API_KEY,
						},
					},
					schema: ductRoutingSchema,
					messages: [
						{
							role: "system",
							content: ductRoutingPrompt,
						},
						{
							role: "user",
							content: JSON.stringify(level),
						},
					],
				});
				console.log("Ducts generated for level", level.name);

				return {
					content: [{ type: "text", text: JSON.stringify(ducts) }],
				};
			},
		);
	}
}

export default {
	fetch(request: Request, env: Env, ctx: ExecutionContext) {
		const url = new URL(request.url);

		if (url.pathname === "/sse" || url.pathname === "/sse/message") {
			return MyMCP.serveSSE("/sse").fetch(request, env, ctx);
		}

		if (url.pathname === "/mcp") {
			return MyMCP.serve("/mcp").fetch(request, env, ctx);
		}

		return new Response("Not found really", { status: 404 });
	},
};
