import { useQueryClient } from "@tanstack/react-query";
import { useRef } from "react";
import type { OpportunityProjection } from "#/api/models";
import { salesKeys } from "@/api/query-keys";
import { Kanban } from "@/components/kanban";
import { pipelineStages, type SalesPageFillters, useGetPipelineTotals } from "../../hooks";
import type { SalesActionRef } from "../forms";
import SalesActionDrawers from "../forms/SalesActionDrawers";
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

	const renderCard = (item: OpportunityProjection) => (
		<Kanban.Card key={item.id}>
			<div className="sales-card-title">{item.title}</div>

			<div className="sales-card-company">{item.company?.name}</div>
		</Kanban.Card>
	);

	return (
		<>
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
