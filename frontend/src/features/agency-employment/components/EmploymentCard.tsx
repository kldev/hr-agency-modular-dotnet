import { useAuthStore } from "#/stores/authStore";
import type { AgencyEmploymentProjection } from "@/api/models";
import { ContractTypeBadge } from "@/components/ui";
import { DetailItem, DetailOverviewHeader } from "@/components/ui/details/DataDetails";
import { formatDate } from "@/utlis/dateUtils";
import { EmploymentActions } from "../pages/components/table/EmploymentActions";
import { useGetAgencyEmployment } from "../pages/hooks";
import { contractRequiresTimeRecord, formatRate, isEnded, isRates } from "../types";

interface Props {
	userId: string;
	name: string;
	onStart: (userId: string) => void;
	onChangeTerms: (employment: AgencyEmploymentProjection) => void;
	onEnd: (employment: AgencyEmploymentProjection) => void;
}

/**
 * What this person works for us on. It is a fact about them, so it is on their page as well as in
 * the register - both read the same hook, and the drawers are still mounted by the page.
 */
export function EmploymentCard({ userId, name, onStart, onChangeTerms, onEnd }: Props) {
	const query = useGetAgencyEmployment(userId);

	const employment = query.data ?? null;

	const showsRate = isRates(useAuthStore((state) => state.user?.role));

	return (
		<section className="data-details-section">
			<div className="data-overview">
				<DetailOverviewHeader
					title="Employment"
					description="What this person works for the agency on, and whether it carries the duty to record hours."
					onAdd={employment || query.isPending ? undefined : () => onStart(userId)}
				/>

				{/* Not asked yet is not the same as nobody on record, and only one of them is news. */}
				{query.isPending ? null : employment ? (
					<>
						<dl className="data-details-list">
							<DetailItem label="Contract">
								<ContractTypeBadge contractType={employment.contractType} />
							</DetailItem>

							<DetailItem label="Weekly hours">
								{employment.weeklyHours === null ? null : `${employment.weeklyHours} h`}
							</DetailItem>

							{/* Not a blank for everybody else: the row is not theirs to see at all. */}
							{showsRate ? (
								<DetailItem label="Rate">
									{employment.rate ? formatRate(employment.rate) : "Not quoted"}
								</DetailItem>
							) : null}

							<DetailItem label="Engaged">
								{formatDate(employment.startsOn)}
								{employment.endsOn ? ` – ${formatDate(employment.endsOn)}` : " – still running"}
							</DetailItem>

							<DetailItem label="Time record">
								{contractRequiresTimeRecord[employment.contractType]
									? "Owes hours every month"
									: "No duty to record hours"}
							</DetailItem>
						</dl>

						{isEnded(employment) ? null : (
							<EmploymentActions
								mode="details"
								userId={userId}
								name={name}
								isEnded={false}
								onChangeTerms={() => onChangeTerms(employment)}
								onEnd={() => onEnd(employment)}
							/>
						)}
					</>
				) : (
					<p className="data-details-empty">
						Nobody has recorded what this person works for us on, so no time sheet is expected of
						them.
					</p>
				)}
			</div>
		</section>
	);
}
