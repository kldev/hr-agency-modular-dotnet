import { fileURLToPath } from "node:url";
import { defineConfig } from "vitest/config";

/*
 * Unit tests for logic without React - building a Zod schema from a form definition, mapping field
 * codes to form keys. Kept apart from vite.config.ts on purpose: the app's plugins (TanStack Start,
 * nitro) have nothing to do in a test of a pure function and only slow it down.
 */
export default defineConfig({
	resolve: {
		alias: {
			"#": fileURLToPath(new URL("./src", import.meta.url)),
			"@": fileURLToPath(new URL("./src", import.meta.url)),
		},
	},
	test: {
		include: ["src/**/*.test.ts"],
		environment: "node",
	},
});
