import { useQueryClient } from "@tanstack/react-query";
import { useRef } from "react";
import type { OpportunityProjection } from "#/api/models";
import { salesKeys } from "@/api/query-keys";
import { Kanban } from "@/components/kanban";
import { pipelineStages, type SalesPageFillters, useGetPipelineTotals } from "../../hooks";
import type { SalesActionRef, SalesActionTypes } from "../forms";
import SalesActionDrawers from "../forms/SalesActionDrawers";
import { SalesMetrics } from "../SalesMetrics";
import { OpportunityCard } from "./OpportunityCard";
import { SalesKanbanColumn } from "./SalesKanbanColumn";

interface SalesKanbanProps {
	filters: SalesPageFillters;
}

export function SalesKanban({ filters }: SalesKanbanProps) {
	const salesRef = useRef<SalesActionRef>(null);
	const client = useQueryClient();

	const { totals } = useGetPipelineTotals(filters);

	// one invalidation refreshes every column and the totals in the headers
	const onSuccess = () => {
		client.invalidateQueries({ queryKey: salesKeys.all });
	};

	const handleAction = (action: SalesActionTypes, item: OpportunityProjection) => {
		if (action === "change-stage") {
			salesRef.current?.changeStage({ id: item.id, stage: item.stage, title: item.title });
			return;
		}

		salesRef.current?.onAction(item.id, action);
	};

	const renderCard = (item: OpportunityProjection) => (
		<OpportunityCard
			key={item.id}
			opportunity={item}
			onAction={(action) => handleAction(action, item)}
		/>
	);

	return (
		<>
			<SalesMetrics totals={totals} />

			<Kanban label="Sales pipeline">
				{pipelineStages.map((stage) => (
					<SalesKanbanColumn
						key={stage}
						stage={stage}
						filters={filters}
						totals={totals[stage]}
						renderCard={renderCard}
					/>
				))}
			</Kanban>

			<SalesActionDrawers ref={salesRef} onSuccess={onSuccess} />
		</>
	);
}
