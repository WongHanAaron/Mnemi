# Card Grouping and Tags

This document describes how to organize flashcards into groups using Obsidian tags and per-card tag overrides in Mnemi.

## Overview

Cards are organized into hierarchical groups (similar to Anki decks) using tags. The system supports:

- **Document-level tags**: applied to all cards in a file
- **Card-level tags**: override document defaults for specific cards
- **Native Obsidian integration**: tags you create in Obsidian are automatically used by Mnemi

## Document-level tags

Document-level tags apply to all cards in the file. They must appear in the **file header, before any QA content**.

### Format

Use any inline Obsidian tags to define groups. Tags are hierarchical, with slashes representing hierarchy levels:

```markdown
#spanish
#verbs

# Common Spanish Verbs

Some introductory text here...

What is "hablar"?::to speak
<!-- mnemi: 7f4a2e | status=review | days=19 | ease=2.4 | due=2026-05-12 | last=good | lapses=1 | reps=14 -->
```

### Tag hierarchy

Tags are hierarchical. Slashes in tag names become colons in group paths. Each segment can use either format:

**Lowercase format** (auto-converted to UpperCamelCase):
- `#spanish` → `Spanish`
- `#spanish/verbs` → `Spanish::Verbs`
- `#spanish/verbs/regular` → `Spanish::Verbs::Regular`

**UpperCamelCase format** (multi-word segments with inferred spaces):
- `#Spanish` → `Spanish`
- `#Spanish/Verbs` → `Spanish::Verbs`
- `#MachineLearning/BayesianInference` → `MachineLearning::BayesianInference`

**Mixed format** is allowed:
- `#spanish/Verbs/regular` → `Spanish::Verbs::Regular`
- `#python/MachineLearning/TimeSeriesAnalysis` → `Python::MachineLearning::TimeSeriesAnalysis`

Each segment must be either all lowercase or UpperCamelCase. The first letter of each segment is always capitalized in the resulting group path.

### Multiple document tags

You can have multiple document-level tags. A card can belong to multiple groups.

**Same-hierarchy tags:** If multiple tags share the same root, the longest/most specific is used:

```markdown
#spanish
#spanish/verbs
#spanish/verbs/regular

# Regular Spanish Verbs

What is "hablar"?::to speak
<!-- mnemi: 7f4a2e | status=review | ... -->
# Group: Spanish::Verbs::Regular (most specific wins)
```

**Independent tags:** If tags have different roots, all apply — the card is a member of both groups:

```markdown
#mathematics/probability
#machinelearning/bayesianinference

# Bayesian Inference in Machine Learning

What is Bayes' theorem?::P(A|B) = P(B|A) × P(A) / P(B)
<!-- mnemi: abc123 | status=review | ... -->
# Groups: Mathematics::Probability AND MachineLearning::BayesianInference
```

You can mix lowercase and UpperCamelCase:

```markdown
#python
#MachineLearning/TimeSeriesAnalysis

# Time Series Forecasting

What is ARIMA?::AutoRegressive Integrated Moving Average
<!-- mnemi: def789 | status=review | ... -->
# Groups: Python AND MachineLearning::TimeSeriesAnalysis
```

## Card-level tag override

Override the document-level group for a specific card using the `tag=` field in the mnemi comment.

### Format

Single tag override:
```markdown
What is "cocinar" (irregular)?::to cook
<!-- mnemi: abc123 | status=new | days=0 | ease=2.5 | due=2026-04-24 | last=new | lapses=0 | reps=0 | tag=Spanish::Verbs::Irregular -->
```

Multiple tags (card belongs to multiple groups):
```markdown
What is Bayes' theorem?::P(A|B) = P(B|A) × P(A) / P(B)
<!-- mnemi: def456 | status=review | ... | tag=Mathematics::Probability,MachineLearning::BayesianInference -->
```

Separate multiple tags with commas. No spaces around commas.

### When to use card-level tags

Use card-level tags when:
- A card belongs to a different group than the rest of the document
- A card needs a more specific group than the document default
- You want to reclassify a single card without changing the whole document
- A card belongs to multiple groups that don't all match the document tags

### Example: mixed groups in one file

```markdown
#spanish
#spanish/verbs

# Spanish Verbs

## Regular verbs

What is "hablar"?::to speak
<!-- mnemi: 7f4a2e | status=review | days=19 | ease=2.4 | due=2026-05-12 | last=good | lapses=1 | reps=14 -->
# Group: Spanish::Verbs (from document tags)

What is "correr"?::to run
<!-- mnemi: def456 | status=review | days=15 | ease=2.3 | due=2026-05-08 | last=good | lapses=0 | reps=12 -->
# Group: Spanish::Verbs (from document tags)

## Irregular verbs

What is "ser"?::to be
<!-- mnemi: ghi789 | status=new | days=0 | ease=2.5 | due=2026-04-24 | last=new | lapses=0 | reps=0 | tag=Spanish::Verbs::Irregular -->
# Group: Spanish::Verbs::Irregular (card-level override)

What is "tener"?::to have
<!-- mnemi: jkl012 | status=new | days=0 | ease=2.5 | due=2026-04-24 | last=new | lapses=0 | reps=0 | tag=Spanish::Verbs::Irregular -->
# Group: Spanish::Verbs::Irregular (card-level override)

What is "bailar" (also a dance)?::to dance
<!-- mnemi: mno345 | status=new | days=0 | ease=2.5 | due=2026-04-24 | last=new | lapses=0 | reps=0 | tag=Spanish::Verbs,Dance::Movement -->
# Groups: Spanish::Verbs AND Dance::Movement (multi-group card)
```

## Tag precedence

When a card's group is determined:

1. **Card-level `tag=` in mnemi comment** (highest priority)
2. **Document-level tags** (fallback)
3. **No group** (if neither is set, card has no group)

Example:

```markdown
#spanish/verbs

What is "hablar"?::to speak
<!-- mnemi: 7f4a2e | status=review | ... -->
# Group: Spanish::Verbs (from document tags)

What is "cocinar" (irregular)?::to cook
<!-- mnemi: abc123 | status=new | ... | tag=Spanish::Verbs::Irregular -->
# Group: Spanish::Verbs::Irregular (mnemi comment overrides document)

What is "bailar" (also a dance)?::to dance
<!-- mnemi: def456 | status=new | ... | tag=Spanish::Verbs,Dance::Movement -->
# Groups: Spanish::Verbs AND Dance::Movement (card is in both groups)
```

## Obsidian integration

Because the grouping system uses native Obsidian tags:

- **Click on tags in Obsidian** to find all cards with that tag across files
- **See tags in the tag pane** to browse your card hierarchy
- **Use tag graph** to visualize relationships between groups
- **Search by tag** using `tag:#spanish` or `tag:#spanish/verbs` in Obsidian's search

This means Mnemi groups are first-class citizens in your Obsidian vault.

## Best practices

### 1. Start broad, go specific

```markdown
#spanish
#spanish/verbs
#spanish/verbs/present-tense
```

Not just:
```markdown
#spanish/verbs/present-tense/irregular/first-person
```

Shorter hierarchies are easier to manage.

### 2. Use document tags for consistency

Put document-level tags in the file header so all cards share a common group. Use card-level overrides only when necessary.

```markdown
#spanish/verbs

# All verbs here are Spanish verbs by default

What is "hablar"?::to speak
<!-- mnemi: 7f4a2e | status=review | ... -->

What is "ser" (irregular)?::to be
<!-- mnemi: abc123 | status=new | ... | tag=Spanish::Verbs::Irregular -->
# Only this one is irregular
```

### 3. Keep hierarchy depth reasonable

Deep hierarchies can become hard to navigate:

Good:
```
Spanish::Verbs::Present
Spanish::Verbs::Past
```

Less ideal:
```
Spanish::Verbs::Present::Regular::FirstPerson::Positive
```

### 4. Use tag naming conventions

Be consistent:

- All lowercase in Obsidian: `#spanish/verbs`
- Mnemi auto-capitalizes: `Spanish::Verbs`
- Use singular or plural consistently (not `#spanish/verbs` and `#spanish/languages`)

### 5. Document your tag structure

In your vault's main README or a dedicated file, document your tag hierarchy so users know what tags to use:

```markdown
# Flashcard Organization

## Tag Structure

- `#language` — language category
  - `#spanish` — Spanish content
    - `#spanish/verbs` — verbs
    - `#spanish/vocab` — vocabulary
    - `#spanish/grammar` — grammar
  - `#french` — French content
- `#science` — science topics
  - etc.
```

## Examples

### Example 1: Single language, multiple skills

```markdown
#spanish

# Spanish Learning

What is "hola"?::hello
<!-- mnemi: 7f4a2e | status=review | days=30 | ease=2.5 | due=2026-05-23 | last=good | lapses=0 | reps=20 -->

What is the verb "to speak"?::hablar
<!-- mnemi: abc123 | status=review | days=15 | ease=2.4 | due=2026-05-08 | last=good | lapses=0 | reps=12 | tag=Spanish::Verbs -->

What is the past tense of "hablar"?::hablé, hablaste, habló
<!-- mnemi: def456 | status=new | days=0 | ease=2.5 | due=2026-04-24 | last=new | lapses=0 | reps=0 | tag=Spanish::Verbs::Past -->
```

### Example 2: Subject-based organization

```markdown
#biology
#biology/cells

# Cell Biology

What is the function of the mitochondria?::energy production (ATP)
<!-- mnemi: 7f4a2e | status=review | days=20 | ease=2.5 | due=2026-05-13 | last=good | lapses=0 | reps=8 -->

What is the function of the nucleus?::DNA storage and control
<!-- mnemi: abc123 | status=review | days=20 | ease=2.4 | due=2026-05-12 | last=good | lapses=1 | reps=7 -->
```

### Example 3: Mixed difficulty levels

```markdown
#spanish/vocab

# Spanish Vocabulary

What is "agua"?::water
<!-- mnemi: 7f4a2e | status=review | days=60 | ease=2.8 | due=2026-06-22 | last=easy | lapses=0 | reps=50 -->

What is "antidisestablishmentarianism" (Spanish)?::antidesestablecimentarismo
<!-- mnemi: abc123 | status=new | days=0 | ease=2.5 | due=2026-04-24 | last=new | lapses=0 | reps=0 | tag=Spanish::Vocab::Advanced -->
```

## Summary

- **Document-level tags** set the default group for all cards in a file
- **Card-level `tag=`** overrides the default for specific cards
- Tags are native Obsidian tags, so they integrate seamlessly with Obsidian's features
- Use the format `Group::Subgroup::Path` for maximum clarity
- Keep hierarchies shallow and consistent for easier management
