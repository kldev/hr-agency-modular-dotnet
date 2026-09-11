import { Button } from "./Button";

type LoadMoreProps = {
	hasNext: boolean;
	loading: boolean;
	onClick: () => void;
};

export function LoadMore({ hasNext, loading, onClick }: LoadMoreProps) {
	if (!hasNext) return null;
	return (
		<div className="load-more">
			<Button className="load-more-button" variant="secondary" loading={loading} onClick={onClick}>
				Load more
			</Button>
		</div>
	);
}
