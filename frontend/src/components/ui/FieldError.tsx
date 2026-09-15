type FieldErrorValue =
	| string
	| {
			message?: string;
			code?: string;
			path?: unknown[];
	  }
	| unknown;

function getErrorMessage(error: FieldErrorValue): string {
	if (typeof error === "string") {
		return error;
	}

	if (error && typeof error === "object" && "message" in error) {
		const message = error.message;

		if (typeof message === "string") {
			return message;
		}
	}

	return String(error);
}

export function FieldError({ errors }: { errors: Array<unknown> }) {
	if (errors.length === 0) {
		return null;
	}

	return (
		<div className="form-field-error" role="alert">
			{errors.map((error, index) => {
				const message = getErrorMessage(error);

				return <div key={`${message}-${index}`}>{message}</div>;
			})}
		</div>
	);
}
