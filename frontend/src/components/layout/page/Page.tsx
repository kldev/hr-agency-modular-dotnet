import { RefreshCcw } from "lucide-react";
import { Button } from "../../ui";
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
	headerAddon?: React.ReactNode;
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
