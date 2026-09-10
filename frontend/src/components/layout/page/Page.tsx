import { RefreshCcw } from "lucide-react";
import { Button, LoadingState } from "../../ui";
import { Breadcrumbs } from "../breadcrumbs";
import "./page.css";

interface Props {
	children: React.ReactNode;
	onRefresh?: (page: number) => void;
	title: string;
	description: string;
	loading?: boolean;
	page?: number;
	emptyState: React.ReactNode;
	isEmpty?: boolean;
}

const Page: React.FC<Props> = ({
	children,
	title,
	description,
	onRefresh,
	loading,
	page,
	isEmpty,
	emptyState,
}) => {
	return (
		<main className="min-w-0 flex-1 overflow-auto">
			<div className="page">
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
							loading={loading && page === 0}
						>
							Refresh
						</Button>
					) : null}
				</header>
			</div>
			{loading ? <LoadingState></LoadingState> : null}
			{isEmpty && !loading ? emptyState : null}
			{children}
		</main>
	);
};

export default Page;
