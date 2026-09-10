import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";

import "./styles/global.css";
import { App } from "./App.tsx";
import { Provider } from "./provider.tsx";

// biome-ignore lint/style/noNonNullAssertion: false
createRoot(document.getElementById("root")!).render(
	<StrictMode>
		<BrowserRouter>
			<Provider>
				<App />
			</Provider>
		</BrowserRouter>
	</StrictMode>,
);
