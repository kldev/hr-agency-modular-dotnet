import type React from "react";
import { useLocation } from "react-router-dom";

const Stub: React.FC = () => {
	const location = useLocation();
	return (
		<div className="p-5">
			<h1 className="text-3xl text-mauve-600 uppercase">
				{location.pathname.toUpperCase().replace("/", "")}
			</h1>
		</div>
	);
};

export default Stub;
