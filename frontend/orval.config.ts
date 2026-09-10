import { defineConfig } from "orval";

const WEB_API_URL = "http://localhost:5000";

const commonOutput = {
	mode: "tags-split" as const,
	client: "react-query" as const,
	httpClient: "axios" as const,
	clean: true,
	prettier: false,
	override: {
		mutator: {
			path: "./src/api/mutator.ts",
			name: "customInstance",
		},
		query: {
			useQuery: true,
			useMutation: true,
		},
	},
	schemas: {
		splitByTags: true,
	},
	// baseUrl: {
	// 	runtime: "env.VITE_API_URL",
	// 	imports: [{ name: "env", importPath: "./" }],
	// },
};

export default defineConfig({
	webapi: {
		input: {
			target: `${WEB_API_URL}/openapi/v1.json`,
		},
		output: {
			...commonOutput,
			target: "./src/api/endpoints",
			schemas: "./src/api/models",
		},
	},
});
