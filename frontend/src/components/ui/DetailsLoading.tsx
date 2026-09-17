type DetailsLoadingProps = {
	id: string;
	isLoading: boolean;
	isError: boolean;
};

export function DetailsLoading({ id, isError, isLoading }: DetailsLoadingProps) {
	if (!id) {
		return (
			<div className="job-application-details">
				<div className="data-details-empty">Record not found.</div>
			</div>
		);
	}

	if (isLoading) {
		return (
			<div className="job-application-details">
				<div className="data-details-loading">Loading ...</div>
			</div>
		);
	}

	if (isError) {
		return (
			<div className="job-application-details">
				<div className="data-details-error">Unable to load the data.</div>
			</div>
		);
	}

	return null;
}
