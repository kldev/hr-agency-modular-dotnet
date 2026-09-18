import type { CurrencyCode, OpportunityStage, SalesPipelineQueryResult } from "#/api/models";
import { formatSalary } from "#/utlis";

export type StageValue = {
	currency: CurrencyCode;
	value: number;
};

export type StageTotals = {
	count: number;
	values: StageValue[];
};

export const pipelineStages: OpportunityStage[] = [
	"New",
	"Viewed",
	"Contacted",
	"Qualified",
	"Proposal",
	"Won",
	"Lost",
];

// totals are grouped by stage AND currency, so one stage can hold several rows;
// the backend does not convert currencies and neither do we
export function groupPipelineTotals(
	rows: SalesPipelineQueryResult[] | undefined,
): Record<OpportunityStage, StageTotals> {
	const totals = {} as Record<OpportunityStage, StageTotals>;

	for (const stage of pipelineStages) {
		totals[stage] = { count: 0, values: [] };
	}

	for (const row of rows ?? []) {
		const stage = totals[row.stage];

		if (!stage) {
			continue;
		}

		stage.count += Number(row.count);
		stage.values.push({ currency: row.currencyCode, value: Number(row.totalExpectedValue) });
	}

	for (const stage of Object.values(totals)) {
		stage.values.sort((left, right) => right.value - left.value);
	}

	return totals;
}

export function sumStageValues(
	totals: Record<OpportunityStage, StageTotals>,
	stages: OpportunityStage[],
): StageValue[] {
	const sums = new Map<CurrencyCode, number>();

	for (const stage of stages) {
		for (const item of totals[stage].values) {
			sums.set(item.currency, (sums.get(item.currency) ?? 0) + item.value);
		}
	}

	return [...sums.entries()]
		.map(([currency, value]) => ({ currency, value }))
		.sort((left, right) => right.value - left.value);
}

// several currencies per stage are listed one after another - the backend does not
// convert them, so a single "total" would be made up
export function formatStageValues(values: StageValue[]): string {
	if (values.length === 0) {
		return "0";
	}

	return values.map((it) => `${formatSalary(it.value)} ${it.currency}`).join(" · ");
}

export function sumStageCount(
	totals: Record<OpportunityStage, StageTotals>,
	stages: OpportunityStage[],
): number {
	return stages.reduce((sum, stage) => sum + totals[stage].count, 0);
}
