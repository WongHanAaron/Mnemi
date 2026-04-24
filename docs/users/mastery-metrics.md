# Mastery Metrics for Mnemi

This document defines two per-deck learning metrics for Mnemi and proposes a compact way to store prior metric state so improvement can be measured after each study session.[1]

## Metric 1: Lapse Rate

**Goal:** Measure how often previously seen cards fail during review.[1]

### Definition

For a given deck and time window, **Lapse Rate** is the ratio of:

- Numerator: non-new review events where the card was marked `Again`.[1]
- Denominator: all non-new review events in that deck, with repetitions allowed.[1]

This is an event-based metric, not a unique-card metric, because repeated failures during a session are meaningful signals of difficulty and should count.[1]

### Formula

$$
\text{LapseRate}_{d,t} = \frac{\#(\text{review events in deck } d \text{ where pre-review status} \neq new \text{ and response}=Again)}{\#(\text{review events in deck } d \text{ where pre-review status} \neq new)}
$$

### Notes

- Use the **pre-review** card state to decide whether a card is non-new.[1][2]
- Count all qualifying review events, even if the same card appears multiple times in the same session.[1]
- Lower values are better.[1]

### Example

If a learner reviews 40 non-new cards in a deck during a session and 6 of those review events are answered `Again`, then:

$$
\text{LapseRate} = \frac{6}{40} = 0.15 = 15\%
$$

## Metric 2: New Card Conversion Rate

This metric was previously discussed as “Mastery Rate,” but **New Card Conversion Rate** is the more precise name because it measures short-term session conversion of new cards rather than long-term mastery.[1]

**Goal:** Measure how effectively new cards become successfully handled during the current session.[1]

### Definition

For a given deck and study session, **New Card Conversion Rate** is the ratio of:

- Numerator: unique cards that **started the session as `new`** and whose **final session outcome** was neither `Again` nor `Hard` (that is, final outcome was `Good` or `Easy`).[1]
- Denominator: unique cards that started the session as `new` and were reviewed at least once in that session.[1][2]

This is a unique-card session metric, not an event metric, because the question is whether a new card was successfully converted by the end of the session, not how many attempts it took.[1]

### Formula

$$
\text{NewCardConversionRate}_{d,s} = \frac{\#(\text{unique cards in deck } d \text{ that started session } s \text{ as } new \text{ and ended with final outcome } Good \text{ or } Easy)}{\#(\text{unique cards in deck } d \text{ that started session } s \text{ as } new \text{ and were reviewed at least once})}
$$

### Notes

- Use each card only once in the numerator and denominator for a given session.[1]
- Determine eligibility using the card’s state **at session start**, not after the first review event.[1][2]
- Higher values are better.[1]

### Example

If 12 unique new cards from a deck were reviewed in a session, and 9 of them finished the session with final outcomes `Good` or `Easy`, then:

$$
\text{NewCardConversionRate} = \frac{9}{12} = 0.75 = 75\%
$$

## Improvement calculations

Improvement should be tracked per deck so the product can show messages such as “Your lapse rate in SpanishVerbs improved by 10%” or “Your new card conversion rate in BiologyCells improved by 8 points.”[3]

### Lapse Rate improvement

Because lower lapse rate is better, improvement is the relative reduction from the previous stored rate:

$$
\text{LapseImprovement}_{d} = \frac{\text{PrevLapseRate}_{d} - \text{CurrentLapseRate}_{d}}{\text{PrevLapseRate}_{d}}
$$

Absolute change can also be useful for messaging:

$$
\Delta \text{LapseRate}_{d} = \text{CurrentLapseRate}_{d} - \text{PrevLapseRate}_{d}
$$

### New Card Conversion Rate improvement

Because higher conversion rate is better, improvement is the relative increase from the previous stored rate:

$$
\text{ConversionImprovement}_{d} = \frac{\text{CurrentConversionRate}_{d} - \text{PrevConversionRate}_{d}}{\text{PrevConversionRate}_{d}}
$$

Absolute change is often easier for users to read:

$$
\Delta \text{ConversionRate}_{d} = \text{CurrentConversionRate}_{d} - \text{PrevConversionRate}_{d}
$$

### Recommended user-facing rule

For user messages, absolute percentage-point change is usually easier to understand, while relative change is better for ranking and analytics.[1] A practical approach is:

- Show users: “Lapse rate down 4 points” or “New card conversion up 9 points.”
- Store internally: both current rate and relative improvement.

## Compact state tracking proposal

Mnemi already stores per-card scheduler state in a compact hidden HTML comment and supports per-card or document-level grouping through tags.[1][3][2] A similar compact strategy can be used for per-deck metric history.[1][3]

### What to store per deck

For each deck, store only the previous baseline values needed for the next comparison:

- Deck identifier.
- Previous lapse rate.
- Previous new card conversion rate.
- Sample size for previous lapse rate, meaning count of non-new review events used to compute it.
- Sample size for previous conversion rate, meaning count of unique new cards reviewed in that session or window.
- Last updated timestamp or session id.

This is enough to compare the newly computed metrics after a session and then overwrite the prior state with the latest accepted baseline.[1]

### Suggested compact format

A compact per-deck record can look like this:

```text
mm deck=SpanishVerbs lr=0.150 ln=40 cr=0.750 cn=12 ts=2026-04-23
```

Where:

- `deck` = deck or normalized group path.[3]
- `lr` = previous lapse rate.
- `ln` = lapse-rate denominator, total non-new review events.
- `cr` = previous new card conversion rate.
- `cn` = conversion-rate denominator, total unique new cards reviewed.
- `ts` = last baseline update date or session id.

If deck names are long, the system can store a short hash or internal deck id instead of the full string:

```text
mm d=sv01 lr=.15 ln=40 cr=.75 cn=12 ts=2026-04-23
```

### JSON alternative

If readability matters more than minimal size, a small JSON object per deck is still compact enough:

```json
{
  "deck": "SpanishVerbs",
  "lr": 0.15,
  "ln": 40,
  "cr": 0.75,
  "cn": 12,
  "ts": "2026-04-23"
}
```

### Update flow after a study session

For each deck touched in the session:

1. Compute the current lapse rate from the session or chosen rolling window.[1]
2. Compute the current new card conversion rate from the session.[1]
3. Load the previous stored metric state for that deck.[3]
4. Compare previous vs current values to generate improvement messages.
5. Decide whether to accept the current values as the new baseline.
6. Overwrite the stored per-deck metric record with the updated values and sample sizes.

### Recommended baseline policy

To avoid noisy improvement messages, only update the stored baseline when the sample is large enough.[1] A simple rule is:

- Update `lr` only if `ln >= 20` non-new review events.
- Update `cr` only if `cn >= 5` or `cn >= 10` unique new cards reviewed.

This prevents small sessions from creating misleading swings in improvement.[1]

## Storage placement options

There are two practical places to store this state compactly.

### Option A: Central metrics store

Keep one metrics file for the whole vault, such as:

```text
.mnemi-metrics
```

With one line per deck:

```text
mm d=SpanishVerbs lr=.15 ln=40 cr=.75 cn=12 ts=2026-04-23
mm d=BiologyCells lr=.08 ln=25 cr=.60 cn=10 ts=2026-04-23
```

This is the simplest option for updates and comparisons across sessions.

### Option B: Embed in a hidden comment in a deck-level note

If Mnemi already uses hidden comments in markdown content, a deck-level note or index note could contain a hidden metrics record such as:

```markdown
<!-- mnemi-metrics deckSpanishVerbs lr.15 ln40 cr.75 cn12 ts2026-04-23 -->
```

This keeps the state human-inspectable and consistent with Mnemi’s existing hidden comment style.[1][2]

## Recommendation

Use **Lapse Rate** and **New Card Conversion Rate** as the two user-facing motivational metrics.[1] Store one compact per-deck baseline record containing the last accepted values and their sample sizes, then update that record after each study session only when the new sample is large enough to be trustworthy.[1][3]