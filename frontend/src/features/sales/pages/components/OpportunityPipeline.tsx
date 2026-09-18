import clsx from "clsx";
import { Check } from "lucide-react";
import type { OpportunityStage } from "#/api/models";
import { Button } from "@/components/ui";
import { salesStageOptions } from "../../types";

interface OpportunityPipelineProps {
	stage: OpportunityStage;
	lostReason: string;
	onStageChange: (stage: OpportunityStage) => void;
}

const stages: OpportunityStage[] = ["New", "Viewed", "Contacted", "Qualified", "Proposal", "Won"];

export function OpportunityPipeline({
	stage,
	lostReason,
	onStageChange,
}: OpportunityPipelineProps) {
	const currentIndex = stages.indexOf(stage);

	return (
		<section className="data-details-section sales-pipeline-panel">
			<div className="data-details-section-header">
				<div>
					<h2>Pipeline stage</h2>
					<p>Current opportunity status</p>
				</div>
			</div>

			{stage === "Lost" ? (
				<div className="sales-pipeline-lost">
					<span>Lost: {lostReason || "no reason given"}</span>

					<Button variant="ghost" onClick={() => onStageChange("New")}>
						Reopen opportunity
					</Button>
				</div>
			) : null}

			<div className="sales-pipeline">
				{stages.map((item, index) => {
					const isCurrent = item === stage;
					const isCompleted = currentIndex >= 0 && index < currentIndex;

					return (
						<Button
							key={item}
							variant={isCurrent ? "secondary" : "ghost"}
							className={clsx("sales-pipeline-stage", {
								"sales-pipeline-stage-current": isCurrent,
								"sales-pipeline-stage-completed": isCompleted,
							})}
							onClick={() => onStageChange(item)}
						>
							{isCompleted ? (
								<span className="sales-pipeline-check">
									<Check size={13} />
								</span>
							) : null}
							{salesStageOptions[item]}
						</Button>
					);
				})}
			</div>
		</section>
	);
}
