# Easing Schedule

This document defines the `easing-schedule` algorithm used by Mnemi for flashcard scheduling. The goal is to match Anki's familiar ease-based scheduling behavior while keeping the scheduler simple and predictable.

## What this means

Mnemi will support the same four answer buttons as Anki:

- **Again**
- **Hard**
- **Good**
- **Easy**

The scheduler will use the same easing-style behavior and a small "fuzz" adjustment so that cards do not all fall due on the same exact day.

## Basic algorithm

Each review card stores the following state values:

- `interval` — current interval in days
- `ease` — ease factor (starting ease)
- `status` — whether the card is new, learning, review, or lapsed

After each review, the next interval is calculated according to the button pressed.

### Good

- Treats the card as successfully recalled
- Multiplies the current interval by the card's current ease factor
- Example: if `interval = 10` and `ease = 2.5`, then next interval becomes `25`

### Easy

- Treats the card as very well recalled
- Uses the same logic as `Good`, then multiplies by an additional easy bonus
- Example: if `interval = 10`, `ease = 2.5`, and `easy bonus = 1.3`, then next interval becomes `32.5` (rounded to nearest whole day)

### Hard

- Treats the card as recalled with difficulty
- Applies a smaller multiplier than `Good`
- Example: if `interval = 10`, `ease = 2.5`, and `hard factor = 1.2`, then next interval becomes `12`

### Again

- Treats the card as forgotten or incorrect
- Resets the interval to a short relearning delay or to 0 before re-entering the learning flow
- Example: if a review card is forgotten, next interval becomes `1` day or the first relearning step

## Ease factor updates

The algorithm will match Anki’s approach to updating ease.

- A correct response may increase ease slightly
- A hard response may decrease ease slightly
- A failure resets the ease or reduces it significantly for the next review

Example ease updates:

- `Good` preserves ease or increases it modestly
- `Easy` may increase ease more than `Good`
- `Hard` reduces the ease factor slightly
- `Again` may reduce ease more aggressively or leave it unchanged until relearning is complete

## Fuzz

To avoid having many cards due on the same day, Mnemi will apply a small amount of random jitter to review intervals. This is the same idea as Anki’s fuzz feature.

Example fuzz behavior:

- A scheduled interval of `10` days may become `9`, `10`, or `11` days
- The fuzz is small and does not override the core interval calculation
- Fuzz is only applied after the final interval is calculated

## Example transitions

### Example 1: review success

Current state:

- `interval = 8`
- `ease = 2.4`
- button = `Good`

Next interval:

- `8 * 2.4 = 19.2` → rounded to `19`
- apply fuzz: `19 ± 1` → may become `18`, `19`, or `20`

### Example 2: review easy

Current state:

- `interval = 8`
- `ease = 2.4`
- `easy bonus = 1.3`
- button = `Easy`

Next interval:

- `8 * 2.4 * 1.3 = 24.96` → rounded to `25`
- apply fuzz: may become `24`, `25`, or `26`

### Example 3: review hard

Current state:

- `interval = 10`
- `ease = 2.5`
- `hard factor = 1.2`
- button = `Hard`

Next interval:

- `10 * 1.2 = 12`
- apply fuzz: may become `11`, `12`, or `13`

### Example 4: failure

Current state:

- `interval = 20`
- `ease = 2.5`
- button = `Again`

Next interval:

- reset to relearning state
- next interval becomes the first step in the learning sequence, typically `1` day

## Why this design

Matching Anki’s easing schedule makes Mnemi more intuitive for users who are familiar with Anki. It also provides:

- predictable interval growth for mastered material
- a familiar four-button review flow
- an easy way to express whether a card was forgotten, hard, good, or easy
- slightly randomized due dates so reviews do not cluster

## Notes for authors

This is a scheduling decision only. The markdown QA format remains unchanged.

Use the standard answer buttons during review, and Mnemi will map them to easing-schedule behavior behind the scenes.
