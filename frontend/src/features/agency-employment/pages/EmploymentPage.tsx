import { BriefcaseBusiness } from "lucide-react";
import { useRef } from "react";
import { Route } from "#/routes/app/employment";
import { Page } from "@/components/layout";
import { EmptyState } from "@/components/ui";
import {
	ChangeEmploymentTermsDrawer,
	type ChangeEmploymentTermsFormCommand,
	EndEmploymentDrawer,
	type EndEmploymentFormCommand,
	StartEmploymentDrawer,
	type StartEmploymentFormCommand,
} from "../drawers";
import { contractRequiresTimeRecord, registerPurpose } from "../types";
import { EmploymentTable, EmploymentToolbar } from "./components";
import { useGetAgencyEmployments } from "./hooks";

export function EmploymentPage() {
	const startRef = useRef<StartEmploymentFormCommand>(null);
	const termsRef = useRef<ChangeEmploymentTermsFormCommand>(null);
	const endRef = useRef<EndEmploymentFormCommand>(null);

	const navigate = Route.useNavigate();
	const search = Route.useSearch();

	const query = useGetAgencyEmployments();

	/*
	 * Both filters run here rather than on the server: the register is one row per person in the
	 * agency, so it arrives whole, and `?year&month` on the endpoint answers a different question
	 * (who was covered *in that month*) than the toggle asks.
	 */
	const term = (search.search ?? "").trim().toLowerCase();

	const items = (query.data ?? []).filter((employment) => {
		if (search.coveredOnly && !contractRequiresTimeRecord[employment.contractType]) return false;

		if (!term) return true;

		const person =
			`${employment.user.firstName} ${employment.user.lastName} ${employment.user.email}`.toLowerCase();

		return person.includes(term);
	});

	const isEmpty = query.isFetched && items.length === 0;

	const refresh = () => {
		void query.refetch();
	};

	return (
		<>
			<Page
				className="has-mobile-view"
				title="Employment"
				description={registerPurpose}
				onRefresh={refresh}
				loading={query.isPending}
				isEmpty={isEmpty}
				emptyState={
					<EmptyState
						title="Nobody on record yet"
						description="Until somebody is recorded here, the time sheet monitoring cannot tell who has not filled their hours in from who never has to."
					>
						<BriefcaseBusiness size={24} />
					</EmptyState>
				}
			>
				<EmploymentToolbar
					search={search.search ?? ""}
					coveredOnly={search.coveredOnly ?? false}
					onSearchChange={(value) => {
						navigate({ search: (previous) => ({ ...previous, search: value || undefined }) });
					}}
					onCoveredOnlyChange={(value) => {
						navigate({ search: (previous) => ({ ...previous, coveredOnly: value || undefined }) });
					}}
					onAdd={() => startRef.current?.start()}
				/>

				<EmploymentTable
					employments={items}
					onChangeTerms={(employment) => termsRef.current?.changeTerms(employment)}
					onEnd={(employment) => endRef.current?.end(employment)}
				/>
			</Page>

			<StartEmploymentDrawer ref={startRef} onSuccess={refresh} />
			<ChangeEmploymentTermsDrawer ref={termsRef} onSuccess={refresh} />
			<EndEmploymentDrawer ref={endRef} onSuccess={refresh} />
		</>
	);
}
