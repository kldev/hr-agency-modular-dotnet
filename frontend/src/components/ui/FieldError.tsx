export function FieldError({ errors }: { errors: Array<unknown> }) {
	if (errors.length === 0) {
		return null;
	}

	return (
		<div className="form-field-error" role="alert">
			{errors.map((error) => (
				<div key={JSON.stringify(error)}>{String(error)}</div>
			))}
		</div>
	);
}
