import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

import "./styles/global.css";
import { App } from "./App.tsx";

// biome-ignore lint/style/noNonNullAssertion: false
createRoot(document.getElementById("root")!).render(
	<StrictMode>
		<App />
	</StrictMode>,
);
