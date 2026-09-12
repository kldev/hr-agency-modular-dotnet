export function FieldError({ errors }: { errors: Array<unknown> }) {
	if (errors.length === 0) {
		return null;
	}

	return (
		<div className="form-field-error" role="alert">
			{errors.map((error, index) => (
				<div key={index}>{String(error)}</div>
			))}
		</div>
	);
}
