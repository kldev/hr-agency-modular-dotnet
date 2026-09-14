import { ChevronRight } from "lucide-react";

import "./breadcrumbs.css";
import { Link, useMatches } from "@tanstack/react-router";

type BreadcrumbHandle = {
	breadcrumb?: string;
};

declare module "@tanstack/react-router" {
	interface StaticDataRouteOption {
		breadcrumb?: string;
	}
}

const Breadcrumbs = () => {
	const matches = useMatches();

	const breadcrumbs = matches
		.filter((match) => (match?.staticData?.breadcrumb as BreadcrumbHandle)?.breadcrumb)
		.map((match) => ({
			// biome-ignore lint/style/noNonNullAssertion: false
			label: (match.staticData as BreadcrumbHandle).breadcrumb!,
			path: match.pathname,
		}));

	return (
		<nav className="breadcrumbs" aria-label="Breadcrumb">
			{breadcrumbs.map((breadcrumb, index) => {
				const isLast = index === breadcrumbs.length - 1;

				return (
					<span key={breadcrumb.path} className="breadcrumb-item">
						{isLast ? (
							<span aria-current="page">{breadcrumb.label}</span>
						) : (
							<>
								<Link to={breadcrumb.path}>{breadcrumb.label}</Link>

								<ChevronRight className="breadcrumb-separator" size={13} />
							</>
						)}
					</span>
				);
			})}
		</nav>
	);
};

export default Breadcrumbs;
