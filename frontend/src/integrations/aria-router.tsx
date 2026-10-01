import { type NavigateOptions, type ToOptions, useRouter } from "@tanstack/react-router";
import type { ReactNode } from "react";
import { RouterProvider } from "react-aria-components";

declare module "react-aria-components" {
	interface RouterConfig {
		href: ToOptions["to"];
		routerOptions: Omit<NavigateOptions, keyof ToOptions>;
	}
}

/**
 * HeroUI stands on React Aria, and a HeroUI component with an `href` (a breadcrumb, a tab, a menu
 * item) navigates through React Aria's router. Without this it would be a plain anchor and reload
 * the whole app. Where typed `params`/`search` matter, render a TanStack `Link` instead.
 */
export function AriaRouterProvider({ children }: { children: ReactNode }) {
	const router = useRouter();

	return (
		<RouterProvider
			navigate={(to, options) => router.navigate({ ...options, to })}
			useHref={(to) => router.buildLocation({ to }).href}
		>
			{children}
		</RouterProvider>
	);
}
