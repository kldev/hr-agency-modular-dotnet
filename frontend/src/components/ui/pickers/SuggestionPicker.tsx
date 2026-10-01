import { ComboBox, Description, FieldError, Input, type Key, Label, ListBox } from "@heroui/react";
import clsx from "clsx";
import { Check, ChevronDown, Search, X } from "lucide-react";
import { type ReactNode, type RefObject, use, useEffect, useId, useRef, useState } from "react";
import { ComboBoxStateContext } from "react-aria-components";
import "./suggestions.css";

export type SuggestionPickerProps<T> = {
	/** Lets an outside `<label htmlFor>` name the input; generated when not given. */
	id?: string;
	label?: string;
	description?: string;
	placeholder?: string;

	value: string | null;
	inputValue: string;

	onChange: (value: string | null, item?: T) => void;
	onInputChange: (value: string) => void;

	loadSuggestions: (query: string, signal: AbortSignal) => Promise<T[]>;

	/**
	 * The item behind the current `value`, resolved by the parent.
	 *
	 * Needed whenever the picker is mounted with a value but without a label - a wizard step that
	 * was unmounted and mounted again, or an edit form seeded from a record. Without it the input
	 * stays empty although the form holds a valid id.
	 */
	selectedItem?: T | null;

	getKey: (item: T) => string;

	getLabel: (item: T) => string;

	renderItem?: (item: T, selected: boolean) => ReactNode;

	renderEmptyState?: (query: string) => ReactNode;

	getDescription?: (item: T) => string | undefined;

	allowCustomValue?: boolean;

	disabled?: boolean;
	required?: boolean;
	invalid?: boolean;

	error?: string;

	className?: string;

	minQueryLength?: number;

	debounceMs?: number;

	maxSuggestions?: number;

	renderHeader?: (query: string) => ReactNode;

	fieldClassName?: string;

	/**
	 * Closes the menu after a selection. Defaults to true.
	 *
	 * Leaving it open re-runs the search for the label that was just written into the input, so the
	 * user is shown a spinner and has to click elsewhere to get rid of the list.
	 */
	closeOnSelect?: boolean;

	/**
	 * Clears the query instead of writing the selected label into the input.
	 *
	 * For pickers that collect several items and keep their own list below the field, where the
	 * input is a search box rather than the display of the current value.
	 */
	clearInputOnSelect?: boolean;
};

type ComboBoxState = NonNullable<React.ContextType<typeof ComboBoxStateContext>>;

const MENU_GAP = 4;
const MENU_MAX_HEIGHT = 320;
const VIEWPORT_PADDING = 8;

/*
 * React Aria owns the open state and offers no prop to drive it, so the picker reaches the state
 * through its context for the two moves it makes itself: closing after a pick that does not change
 * the value, and reopening after "clear". The collection is also rendered once in a hidden tree,
 * where there is no state - that render must not wipe the reference.
 */
function ComboBoxStateBridge({ stateRef }: { stateRef: RefObject<ComboBoxState | null> }) {
	const state = use(ComboBoxStateContext);

	useEffect(() => {
		if (state) {
			stateRef.current = state;
		}
	});

	return null;
}

/**
 * A typeahead over an API on HeroUI's ComboBox. The list is the server's answer, so the combo box
 * does no filtering of its own (`items` is controlled); this component adds the debounce, the
 * cancellation of stale requests, the minimum query length and the label for a bare id.
 *
 * Both the selection and the text are controlled, so React Aria leaves syncing them to us. It also
 * reports a `null` selection on blur whenever the selected item is not in the current page of
 * suggestions - which is nearly always, since the list is refetched per query - so `null` from it
 * is ignored: clearing happens through typing or the clear button, both handled here.
 */
export function SuggestionPicker<T extends object>({
	id,
	label,
	description,
	placeholder = "Search...",
	value,
	inputValue,
	onChange,
	onInputChange,
	loadSuggestions,
	selectedItem,
	getKey,
	getLabel,
	renderItem,
	renderEmptyState,
	getDescription,
	allowCustomValue = true,
	disabled = false,
	required = false,
	invalid = false,
	error,
	className = "",
	minQueryLength = 0,
	debounceMs = 250,
	maxSuggestions = 8,
	renderHeader,
	fieldClassName = "",
	closeOnSelect = true,
	clearInputOnSelect = false,
}: SuggestionPickerProps<T>) {
	const generatedId = useId();

	const inputId = id ?? `suggestion-picker-${generatedId}`;

	const inputRef = useRef<HTMLInputElement>(null);
	const stateRef = useRef<ComboBoxState | null>(null);

	const abortControllerRef = useRef<AbortController | null>(null);
	const requestIdRef = useRef(0);

	const [suggestions, setSuggestions] = useState<T[]>([]);
	const [isOpen, setIsOpen] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [menuRequest, setMenuRequest] = useState<"open" | "close" | null>(null);

	const selectedItemRef = useRef<T | undefined>(undefined);
	const clearInputAfterSelectRef = useRef(false);
	const inputValueRef = useRef(inputValue);
	const syncRef = useRef({ getKey, getLabel, onInputChange });

	/*
	 * =========================================================
	 * Load suggestions
	 * =========================================================
	 */

	useEffect(() => {
		if (!isOpen) {
			return;
		}

		if (inputValue.length < minQueryLength) {
			setSuggestions([]);
			setIsLoading(false);
			return;
		}

		const timeoutId = window.setTimeout(() => {
			const requestId = ++requestIdRef.current;

			abortControllerRef.current?.abort();

			const controller = new AbortController();

			abortControllerRef.current = controller;

			setIsLoading(true);

			loadSuggestions(inputValue, controller.signal)
				.then((items) => {
					if (controller.signal.aborted || requestId !== requestIdRef.current) {
						return;
					}

					setSuggestions(items.slice(0, maxSuggestions));
				})
				.catch((error) => {
					if (error instanceof DOMException && error.name === "AbortError") {
						return;
					}

					if (!controller.signal.aborted && requestId === requestIdRef.current) {
						setSuggestions([]);
					}
				})
				.finally(() => {
					if (!controller.signal.aborted && requestId === requestIdRef.current) {
						setIsLoading(false);
					}
				});
		}, debounceMs);

		return () => {
			window.clearTimeout(timeoutId);
		};
	}, [inputValue, isOpen, minQueryLength, debounceMs, maxSuggestions, loadSuggestions]);

	useEffect(() => {
		return () => {
			abortControllerRef.current?.abort();
		};
	}, []);

	useEffect(() => {
		if (!value) {
			selectedItemRef.current = undefined;
			return;
		}

		if (clearInputAfterSelectRef.current) {
			clearInputAfterSelectRef.current = false;
			selectedItemRef.current = undefined;
			return;
		}

		const matched = suggestions.find((item) => getKey(item) === value);

		if (matched) {
			selectedItemRef.current = matched;

			const label = getLabel(matched);

			if (inputValue !== label) {
				onInputChange(label);
			}
		}
	}, [value, suggestions, getKey, getLabel, inputValue, onInputChange]);

	/*
	 * =========================================================
	 * Mirror the resolved selection into the input
	 * =========================================================
	 *
	 * The parent resolves `value` into `selectedItem` (a cached query), so this only has to copy
	 * its label into an input that has none yet. The callbacks are read through a ref so a new
	 * inline function on every parent render cannot re-run it.
	 */

	useEffect(() => {
		syncRef.current = { getKey, getLabel, onInputChange };
		inputValueRef.current = inputValue;
	});

	useEffect(() => {
		if (!value || !selectedItem) {
			return;
		}

		const sync = syncRef.current;

		if (sync.getKey(selectedItem) !== value) {
			return;
		}

		selectedItemRef.current = selectedItem;

		if (!inputValueRef.current) {
			sync.onInputChange(sync.getLabel(selectedItem));
		}
	}, [value, selectedItem]);

	/*
	 * Runs after the render that follows a pick or a clear, so the state read here already knows
	 * about it (the bridge's effect, a child's, has run first).
	 */
	useEffect(() => {
		if (!menuRequest) {
			return;
		}

		const state = stateRef.current;

		if (menuRequest === "close" && state?.isOpen) {
			// `setOpen` and not `close`: the latter commits the input, which is already settled
			state.setOpen(false);
		}

		if (menuRequest === "open" && state && !state.isOpen) {
			state.open(null, "manual");
		}

		setMenuRequest(null);
	}, [menuRequest]);

	/*
	 * =========================================================
	 * Actions
	 * =========================================================
	 */
	const selectItem = (item: T) => {
		const key = getKey(item);

		selectedItemRef.current = item;

		onChange(key, item);

		if (clearInputOnSelect) {
			clearInputAfterSelectRef.current = true;
			onInputChange("");
			setSuggestions([]);
		} else {
			onInputChange(getLabel(item));
		}

		// React Aria closes by itself only when the value changes, which a search box never does,
		// and always when it does - so both directions are asked for explicitly
		setMenuRequest(closeOnSelect ? "close" : "open");
	};

	const clear = () => {
		selectedItemRef.current = undefined;

		onChange(null);
		onInputChange("");

		setSuggestions([]);

		inputRef.current?.focus();
		setIsLoading(true);
		setMenuRequest("open");
	};

	const handleInputChange = (nextValue: string) => {
		if (selectedItemRef.current && nextValue !== getLabel(selectedItemRef.current)) {
			selectedItemRef.current = undefined;

			if (!allowCustomValue) {
				onChange(null);
			}
		}

		onInputChange(nextValue);

		// typing opens the list - unless a pick in the same event has just asked to close it
		setMenuRequest((current) => current ?? "open");
	};

	const handleSelectionChange = (key: Key | null) => {
		if (key === null) {
			return;
		}

		const item = suggestions.find((candidate) => getKey(candidate) === String(key));

		if (item) {
			selectItem(item);
		}
	};

	const handleOpenChange = (open: boolean) => {
		if (open && !isOpen) {
			// nothing is said about an empty list until the request for it has answered
			setIsLoading(true);
		}

		setIsOpen(open);
	};

	const renderEmpty = () => {
		if (isLoading) {
			return null;
		}

		if (inputValue.length < minQueryLength) {
			return (
				<div className="suggestion-picker-empty">
					<div className="suggestion-picker-empty-description">
						Enter at least {minQueryLength} characters to search.
					</div>
				</div>
			);
		}

		if (renderEmptyState) {
			return renderEmptyState(inputValue);
		}

		return (
			<div className="suggestion-picker-empty">
				<div className="suggestion-picker-empty-title">No suggestions found</div>

				<div className="suggestion-picker-empty-description">
					{inputValue ? "Try a different search term." : "No suggestions available."}
				</div>
			</div>
		);
	};

	/*
	 * =========================================================
	 * Render
	 * =========================================================
	 */

	return (
		<ComboBox<T>
			id={inputId}
			className={clsx("suggestion-picker", className)}
			items={suggestions}
			inputValue={inputValue}
			onInputChange={handleInputChange}
			selectedKey={value || null}
			onSelectionChange={handleSelectionChange}
			onOpenChange={handleOpenChange}
			// Opening is ours (focus, typing, clear): React Aria's own "focus"/"input" modes reopen
			// the list right after Escape or blur whenever the text is not a selected item's label,
			// which with server-side results is most of the time.
			menuTrigger="manual"
			onFocus={() => setMenuRequest("open")}
			allowsEmptyCollection
			allowsCustomValue={allowCustomValue}
			shouldFocusWrap
			isDisabled={disabled}
			isRequired={required}
			isInvalid={invalid || Boolean(error)}
			validationBehavior="aria"
		>
			<ComboBoxStateBridge stateRef={stateRef} />

			{label && (
				<Label className="form-label">
					{label}

					{required && <span aria-hidden="true"> *</span>}
				</Label>
			)}

			<div className={fieldClassName}>
				<ComboBox.InputGroup className="suggestion-picker-control">
					<Search size={16} aria-hidden="true" className="suggestion-picker-icon" />

					<Input ref={inputRef} placeholder={placeholder} className="suggestion-picker-input" />

					{inputValue && (
						// A plain button: a React Aria one inside the combo box would become its trigger.
						<button
							type="button"
							className="suggestion-picker-clear"
							aria-label="Clear selection"
							disabled={disabled}
							onMouseDown={(event) => {
								event.preventDefault();
							}}
							onClick={clear}
						>
							<X size={15} />
						</button>
					)}

					<ComboBox.Trigger className="suggestion-picker-trigger">
						<ChevronDown size={16} className="suggestion-picker-chevron" />
					</ComboBox.Trigger>
				</ComboBox.InputGroup>

				{description && !error && <Description className="form-hint">{description}</Description>}

				{error && <FieldError className="form-error">{error}</FieldError>}
			</div>

			<ComboBox.Popover
				className="suggestion-picker-menu"
				offset={MENU_GAP}
				maxHeight={MENU_MAX_HEIGHT}
				containerPadding={VIEWPORT_PADDING}
			>
				{renderHeader && <div className="suggestion-picker-header">{renderHeader(inputValue)}</div>}

				<ListBox<T> className="suggestion-picker-list" renderEmptyState={renderEmpty}>
					{(item) => {
						const key = getKey(item);
						const selected = key === value;
						const itemDescription = getDescription?.(item);

						return (
							<ListBox.Item
								id={key}
								textValue={getLabel(item)}
								className="suggestion-picker-option"
							>
								{renderItem ? (
									renderItem(item, selected)
								) : (
									<div className="suggestion-picker-default-item">
										<div className="suggestion-picker-item-content">
											<div className="suggestion-picker-item-label">{getLabel(item)}</div>

											{itemDescription && (
												<div className="suggestion-picker-item-description">{itemDescription}</div>
											)}
										</div>

										{selected && <Check size={16} className="suggestion-picker-item-check" />}
									</div>
								)}
							</ListBox.Item>
						);
					}}
				</ListBox>
			</ComboBox.Popover>

			{allowCustomValue && inputValue && !value && (
				<div className="suggestion-picker-custom-hint">You can enter a custom value.</div>
			)}
		</ComboBox>
	);
}
