import { ShieldAlert } from "lucide-react";
import "./common.css";
import type React from "react";

const AccessDeniedPage: React.FC = () => {
	return (
		<div className="technical-page">
			<div className="technical-page-title">
				<div className="flex items-center flex-col align-middle text-orange-800 text-4xl">
					<ShieldAlert size={48} />
					Access denied
					<p className="technical-page-sub font-extralight text-xl p-5">
						You are not authorized to view this page
					</p>
				</div>
			</div>
		</div>
	);
};

export default AccessDeniedPage;
