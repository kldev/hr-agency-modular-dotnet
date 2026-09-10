import { ChevronRight } from "lucide-react";
import type React from "react";
import "./breadcrumbs.css";

const Breadcrumbs: React.FC = () => {
	return (
		<nav className="breadcrumbs" aria-label="Breadcrumb">
			<a href="/sales">Sales</a>
			<ChevronRight className="breadcrumb-separator" size={13} />
			<span className="breadcrumb-current" aria-current="page">
				Companies
			</span>
		</nav>
	);
};

export default Breadcrumbs;
