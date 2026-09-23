import { RefreshCcw } from "lucide-react";
import { Button } from "../../ui";
import { Breadcrumbs } from "../breadcrumbs";

interface Props {
	children: React.ReactNode;
	onRefresh?: (page: number) => void;
	title: string;
	description: string;
	loading?: boolean;
	page?: number;
	emptyState: React.ReactNode;
	isEmpty?: boolean;
	headerAddon?: React.ReactNode;
	className?: string;
	/** Drops the content width cap - for a board, which has more columns than a 2K screen fits. */
	wide?: boolean;
}

const Page: React.FC<Props> = ({
	children,
	title,
	description,
	onRefresh,
	loading,
	isEmpty,
	emptyState,
	headerAddon,
	className,
	wide,
}) => {
	return (
		<main className={["min-w-0 flex-1 overflow-auto", className].join(" ")}>
			<div className={wide ? "page page-wide" : "page"}>
				<Breadcrumbs />
				<header className="page-header">
					<div>
						<h1 className="page-title">{title}</h1>
						<p className="page-description">{description}</p>
					</div>

					{onRefresh ? (
						<Button
							variant="secondary"
							icon={<RefreshCcw size={15} />}
							onClick={() => onRefresh(0)}
							loading={loading}
						>
							Refresh
						</Button>
					) : null}
					{headerAddon}
				</header>

				{children}
				{isEmpty && !loading ? emptyState : null}
			</div>
		</main>
	);
};

export default Page;
