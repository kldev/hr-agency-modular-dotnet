import { ConstructionIcon } from "lucide-react";

export function WorkInProgress() {
	return (
		<div className="flex items-center flex-col align-middle text-orange-800 text-xl">
			<ConstructionIcon />
			<h1>Work in progress</h1>
		</div>
	);
}
