import type React from "react";
import "./common.css";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui";
import { ROUTES } from "@/routes";

const NotFoundPage: React.FC = () => {
	const naviage = useNavigate();
	return (
		<div className="technical-page">
			<div className="technical-page-title">
				<div className="flex flex-col items-center text-center text-gray-500 gap-3">
					<h1 className="mt-2 text-9xl font-medium">404</h1>
					<h1 className="mt-2 text-4xl font-medium">Page not found</h1>
					<p className="technical-page-sub text-xl font-extralight p-5">
						The page you are looking for doesn&apos;t exist or has been moved.
						<br />
						Please check the URL or return to the previous page.
					</p>
					<Button variant="back" onClick={() => naviage(ROUTES.DASHBOARD)}>
						Back to Home
					</Button>
				</div>
			</div>
		</div>
	);
};

export default NotFoundPage;
