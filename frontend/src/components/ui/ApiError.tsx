import type { BadRequestDetails } from "@/api/models";

export function ApiError({ error }: { error: BadRequestDetails | null }) {
	if (!error) {
		return null;
	}

	return (
		<div className="form-error" role="alert">
			<strong>
				{error.title ?? "An unexpected error occurred. Please contact your administrator."}
			</strong>

			{error.detail && <div className="text-xl text-muted!">{error.detail}</div>}

			{error.validationErrors?.length ? (
				<ul className="text-xl">
					{error.validationErrors.map((message) => (
						<li key={message}>* {message}</li>
					))}
				</ul>
			) : null}
		</div>
	);
}
