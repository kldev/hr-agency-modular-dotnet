import {
	Check,
	ChevronDown,
	LoaderCircle,
	Search,
	X,
} from "lucide-react";
import {

	type ReactNode,
	useCallback,
	useEffect,
	useId,
	useLayoutEffect,
	useRef,
	useState,
} from "react";
import "./suggestions.css";
import { createPortal } from "react-dom";

export type SuggestionPickerProps<T> = {
	label?: string;
	description?: string;
	placeholder?: string;

	value: string | null;
	inputValue: string;

	onChange: (value: string | null, item?: T) => void;
	onInputChange: (value: string) => void;

	loadSuggestions: (query: string, signal: AbortSignal) => Promise<T[]>;

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
};

type MenuPosition = {
	top: number;
	left: number;
	width: number;
	maxHeight: number;
};

const MENU_GAP = 4;
const MENU_MAX_HEIGHT = 320;
const VIEWPORT_PADDING = 8;

export function SuggestionPicker<T>({
	label,
	description,
	placeholder = "Search...",
	value,
	inputValue,
	onChange,
	onInputChange,
	loadSuggestions,
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
}: SuggestionPickerProps<T>) {
	const generatedId = useId();

	const inputId = `suggestion-picker-${generatedId}`;
	const listboxId = `${inputId}-listbox`;

	const rootRef = useRef<HTMLDivElement>(null);
	const controlRef = useRef<HTMLDivElement>(null);
	const menuRef = useRef<HTMLDivElement>(null);
	const inputRef = useRef<HTMLInputElement>(null);

	const abortControllerRef = useRef<AbortController | null>(null);
	const requestIdRef = useRef(0);

	const [suggestions, setSuggestions] = useState<T[]>([]);
	const [isOpen, setIsOpen] = useState(false);
	const [isLoading, setIsLoading] = useState(true);
	const [highlightedIndex, setHighlightedIndex] = useState(-1);

	const [menuPosition, setMenuPosition] = useState<MenuPosition | null>(null);

	const selectedItemRef = useRef<T | undefined>(undefined);

	/*
	 * =========================================================
	 * Menu positioning
	 * =========================================================
	 */

	const updateMenuPosition = useCallback(() => {
		const control = controlRef.current;

		if (!control) {
			return;
		}

		const rect = control.getBoundingClientRect();

		const viewportHeight = window.innerHeight;
		const viewportWidth = window.innerWidth;

		const spaceBelow =
			viewportHeight - rect.bottom - MENU_GAP - VIEWPORT_PADDING;

		const spaceAbove =
			rect.top - MENU_GAP - VIEWPORT_PADDING;

		const preferredHeight = Math.min(
			MENU_MAX_HEIGHT,
			Math.max(spaceBelow, spaceAbove),
		);

		const shouldOpenAbove =
			spaceBelow < 180 && spaceAbove > spaceBelow;

		const top = shouldOpenAbove
			? Math.max(
				VIEWPORT_PADDING,
				rect.top - MENU_GAP - preferredHeight,
			)
			: rect.bottom + MENU_GAP;

		const maxHeight = Math.max(
			120,
			Math.min(
				MENU_MAX_HEIGHT,
				shouldOpenAbove ? spaceAbove : spaceBelow,
			),
		);

		const width = rect.width;

		const left = Math.min(
			Math.max(VIEWPORT_PADDING, rect.left),
			viewportWidth - width - VIEWPORT_PADDING,
		);

		setMenuPosition({
			top,
			left,
			width,
			maxHeight,
		});
	}, []);

	/*
	 * Position immediately when opening.
	 *
	 * useLayoutEffect prevents the dropdown from being
	 * painted first at position 0,0.
	 */
	useLayoutEffect(() => {
		if (!isOpen) {
			setMenuPosition(null);
			return;
		}

		updateMenuPosition();
	}, [isOpen, updateMenuPosition]);

	/*
	 * Reposition while scrolling/resizing.
	 *
	 * `capture: true` is important because the picker can
	 * be inside an overflow-auto container.
	 */
	useEffect(() => {
		if (!isOpen) {
			return;
		}

		const handleScroll = () => {
			updateMenuPosition();
		};

		const handleResize = () => {
			updateMenuPosition();
		};

		window.addEventListener("scroll", handleScroll, true);
		window.addEventListener("resize", handleResize);

		return () => {
			window.removeEventListener("scroll", handleScroll, true);
			window.removeEventListener("resize", handleResize);
		};
	}, [isOpen, updateMenuPosition]);

	/*
	 * Keep menu width synchronized with the control.
	 */
	useEffect(() => {
		if (!isOpen || !controlRef.current) {
			return;
		}

		const observer = new ResizeObserver(() => {
			updateMenuPosition();
		});

		observer.observe(controlRef.current);

		return () => {
			observer.disconnect();
		};
	}, [isOpen, updateMenuPosition]);

	/*
	 * =========================================================
	 * Selected item synchronization
	 * =========================================================
	 */

	useEffect(() => {
		if (!value) {
			selectedItemRef.current = undefined;
			return;
		}

		const selectedItem = suggestions.find(
			(item) => getKey(item) === value,
		);

		if (selectedItem) {
			selectedItemRef.current = selectedItem;

			const label = getLabel(selectedItem);

			if (inputValue !== label) {
				onInputChange(label);
			}
		}
	}, [
		value,
		suggestions,
		getKey,
		getLabel,
		inputValue,
		onInputChange,
	]);

	/*
	 * =========================================================
	 * Click outside
	 * =========================================================
	 *
	 * Because the menu lives in document.body, rootRef alone
	 * is not enough. We have to consider menuRef as well.
	 */

	useEffect(() => {
		if (!isOpen) {
			return;
		}

		const handlePointerDown = (event: PointerEvent) => {
			const target = event.target as Node;

			const clickedInsideRoot =
				rootRef.current?.contains(target);

			const clickedInsideMenu =
				menuRef.current?.contains(target);

			if (!clickedInsideRoot && !clickedInsideMenu) {
				setIsOpen(false);
				setHighlightedIndex(-1);
			}
		};

		document.addEventListener(
			"pointerdown",
			handlePointerDown,
		);

		return () => {
			document.removeEventListener(
				"pointerdown",
				handlePointerDown,
			);
		};
	}, [isOpen]);

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
			setHighlightedIndex(-1);

			loadSuggestions(inputValue, controller.signal)
				.then((items) => {
					if (
						controller.signal.aborted ||
						requestId !== requestIdRef.current
					) {
						return;
					}

					setSuggestions(
						items.slice(0, maxSuggestions),
					);
				})
				.catch((error) => {
					if (
						error instanceof DOMException &&
						error.name === "AbortError"
					) {
						return;
					}

					if (
						!controller.signal.aborted &&
						requestId === requestIdRef.current
					) {
						setSuggestions([]);
					}
				})
				.finally(() => {
					if (
						!controller.signal.aborted &&
						requestId === requestIdRef.current
					) {
						setIsLoading(false);
					}
				});
		}, debounceMs);

		return () => {
			window.clearTimeout(timeoutId);
		};
	}, [
		inputValue,
		isOpen,
		minQueryLength,
		debounceMs,
		maxSuggestions,
		loadSuggestions,
	]);

	useEffect(() => {
		return () => {
			abortControllerRef.current?.abort();
		};
	}, []);

	/*
	 * =========================================================
	 * Actions
	 * =========================================================
	 */

	const selectItem = (item: T) => {
		const key = getKey(item);
		const label = getLabel(item);

		selectedItemRef.current = item;

		onChange(key, item);
		onInputChange(label);

		setIsOpen(false);
		setHighlightedIndex(-1);
	};

	const clear = () => {
		selectedItemRef.current = undefined;

		onChange(null);
		onInputChange("");

		setSuggestions([]);
		setHighlightedIndex(-1);

		inputRef.current?.focus();
		setIsLoading(true)
		setIsOpen(true);
	};

	const handleInputChange = (
		event: React.ChangeEvent<HTMLInputElement>,
	) => {
		const nextValue = event.target.value;

		if (
			selectedItemRef.current &&
			nextValue !==
			getLabel(selectedItemRef.current)
		) {
			selectedItemRef.current = undefined;

			if (!allowCustomValue) {
				onChange(null);
			}
		}

		onInputChange(nextValue);
		setIsOpen(true);
		setHighlightedIndex(-1);
	};

	const handleFocus = () => {
		if (disabled) {
			return;
		}

		setIsLoading(true);
		setIsOpen(true);
	};

	const handleKeyDown = (
		event: React.KeyboardEvent<HTMLInputElement>,
	) => {
		if (disabled) {
			return;
		}

		if (event.key === "ArrowDown") {
			event.preventDefault();

			if (!isOpen) {
				setIsOpen(true);
				return;
			}

			setHighlightedIndex((current) => {
				if (suggestions.length === 0) {
					return -1;
				}

				return current >= suggestions.length - 1
					? 0
					: current + 1;
			});

			return;
		}

		if (event.key === "ArrowUp") {
			event.preventDefault();

			setHighlightedIndex((current) => {
				if (suggestions.length === 0) {
					return -1;
				}

				return current <= 0
					? suggestions.length - 1
					: current - 1;
			});

			return;
		}

		if (event.key === "Enter") {
			if (
				isOpen &&
				highlightedIndex >= 0 &&
				highlightedIndex < suggestions.length
			) {
				event.preventDefault();

				selectItem(
					suggestions[highlightedIndex],
				);
			}

			return;
		}

		if (event.key === "Escape") {
			event.preventDefault();

			setIsOpen(false);
			setHighlightedIndex(-1);

			return;
		}

		if (event.key === "Tab") {
			setIsOpen(false);
			setHighlightedIndex(-1);
		}
	};

	const selectedKey = value;

	const showEmptyState =
		!isLoading &&
		suggestions.length === 0 &&
		inputValue.length >= minQueryLength;

	/*
	 * =========================================================
	 * Render
	 * =========================================================
	 */

	return (
		<div
			ref={rootRef}
			className={`suggestion-picker ${className}`}
		>
			{label && (
				<label
					htmlFor={inputId}
					className="form-label"
				>
					{label}

					{required && (
						<span aria-hidden="true">
							{" "}
							*
						</span>
					)}
				</label>
			)}

			<div className={fieldClassName}>
				<div
					ref={controlRef}
					className={[
						"suggestion-picker-control",
						isOpen
							? "suggestion-picker-control-open"
							: "",
						invalid
							? "suggestion-picker-control-invalid"
							: "",
						disabled
							? "suggestion-picker-control-disabled"
							: "",
					]
						.filter(Boolean)
						.join(" ")}
				>
					<Search
						size={16}
						aria-hidden="true"
						className="suggestion-picker-icon"
					/>

					<input
						ref={inputRef}
						id={inputId}
						type="text"
						role="combobox"
						aria-expanded={isOpen}
						aria-controls={
							isOpen
								? listboxId
								: undefined
						}
						aria-autocomplete="list"
						aria-activedescendant={
							highlightedIndex >= 0
								? `${listboxId}-option-${highlightedIndex}`
								: undefined
						}
						aria-invalid={invalid}
						aria-required={required}
						disabled={disabled}
						value={inputValue}
						placeholder={placeholder}
						className="suggestion-picker-input"
						onFocus={handleFocus}
						onChange={handleInputChange}
						onKeyDown={handleKeyDown}
					/>
					{/* 
					{isLoading && (
						<LoaderCircle className="suggestion-picker-spinner size-4 animate-spin" />
					)} */}

					{!isLoading && inputValue && (
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

					<button
						type="button"
						tabIndex={-1}
						className="suggestion-picker-trigger"
						aria-label="Show suggestions"
						disabled={disabled}
						onMouseDown={(event) => {
							event.preventDefault();
						}}
						onClick={() => {
							inputRef.current?.focus();
							setIsOpen(
								(current) => !current,
							);
						}}
					>
						<ChevronDown
							size={16}
							className={
								isOpen
									? "suggestion-picker-chevron-open"
									: ""
							}
						/>
					</button>
				</div>

				{description && !error && (
					<div className="form-hint">
						{description}
					</div>
				)}

				{error && (
					<div className="form-error">
						{error}
					</div>
				)}
			</div>

			{isOpen &&
				menuPosition &&
				createPortal(
					<div
						ref={menuRef}
						id={listboxId}
						role="listbox"
						className="suggestion-picker-menu"
						style={{
							top: menuPosition.top,
							left: menuPosition.left,
							width: menuPosition.width,
							maxHeight:
								menuPosition.maxHeight,
						}}
					>
						{renderHeader && (
							<div className="suggestion-picker-header">
								{renderHeader(inputValue)}
							</div>
						)}

						{/* {isLoading && (
							<div className="suggestion-picker-state">
								<LoaderCircle
									size={16}
									className="suggestion-picker-state-spinner"
								/>

								<span>
									Loading suggestions...
								</span>
							</div>
						)} */}

						{suggestions.length > 0 && (
							<div className="suggestion-picker-list">
								{suggestions.map((item, index) => {
									const key = getKey(item);

									const selected = key === selectedKey;
									const highlighted = index === highlightedIndex;

									const itemDescription = getDescription?.(item);

									return (
										<button
											key={key}
											id={`${listboxId}-option-${index}`}
											type="button"
											role="option"
											aria-selected={selected}
											className={[
												"suggestion-picker-option",
												highlighted
													? "suggestion-picker-option-highlighted"
													: "",
												selected
													? "suggestion-picker-option-selected"
													: "",
											]
												.filter(Boolean)
												.join(" ")}
											onMouseEnter={() => {
												setHighlightedIndex(index);
											}}
											onMouseDown={(event) => {
												event.preventDefault();
											}}
											onClick={() => {
												selectItem(item);
											}}
										>
											{renderItem ? (
												renderItem(item, selected)
											) : (
												<div className="suggestion-picker-default-item">
													<div className="suggestion-picker-item-content">
														<div className="suggestion-picker-item-label">
															{getLabel(item)}
														</div>

														{itemDescription && (
															<div className="suggestion-picker-item-description">
																{itemDescription}
															</div>
														)}
													</div>

													{selected && (
														<Check
															size={16}
															className="suggestion-picker-item-check"
														/>
													)}
												</div>
											)}
										</button>
									);
								})}
							</div>
						)}

						{showEmptyState &&
							(renderEmptyState ? (
								renderEmptyState(
									inputValue,
								)
							) : (
								<div className="suggestion-picker-empty">
									<div className="suggestion-picker-empty-title">
										No suggestions
										found
									</div>

									{inputValue ? (
										<div className="suggestion-picker-empty-description">
											Try a
											different
											search
											term.
										</div>
									) : (
										<div className="suggestion-picker-empty-description">
											No
											suggestions
											available.
										</div>
									)}
								</div>
							))}

						{!isLoading &&
							inputValue.length <
							minQueryLength && (
								<div className="suggestion-picker-empty">
									<div className="suggestion-picker-empty-description">
										Enter at
										least{" "}
										{
											minQueryLength
										}{" "}
										characters
										to
										search.
									</div>
								</div>
							)}
					</div>,
					document.body,
				)}

			{allowCustomValue &&
				inputValue &&
				!value && (
					<div className="suggestion-picker-custom-hint">
						You can enter a custom value.
					</div>
				)}
		</div>
	);
}