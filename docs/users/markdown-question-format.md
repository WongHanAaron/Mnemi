# Markdown Question Format for Mnemi

This document describes how to write markdown files in an Obsidian repository so that the Mnemi flashcard system can identify questions, answers, cloze items, and multiple-choice content.

## Purpose

Use this format when writing content files that contain both normal Obsidian notes and embedded QA definitions. The syntax is designed to be easy to type, Obsidian-compatible, and unambiguous for parsing.

## Basic rule

- Use `::` as the question/answer separator in single-line definitions.
- Use explicit block markers for multi-line QA content.
- Keep normal note text separate from QA blocks so the parser can distinguish note content from questions.

## Single-line formats

### Single-line QA
```markdown
What year did World War II end?::1945
```

### Single-line cloze
```markdown
The capital of France is {{Paris}}::
```

### Single-line cloze with multiple choice
```markdown
The capital of France is {{Washington|**Paris**}}::
```

### Single-line cloze with multiple questions and options
```markdown
The capital of France is {{Washington|**Paris**}} and is {{100}} years old::
```

### Single-line multiple choice
```markdown
Which element has atomic number 8?::Carbon|Nitrogen|*Oxygen*|Fluorine
```

## Multi-line formats

Use multi-line blocks when the question or answer is long, contains special characters, or needs embedded images.

### Multi-line QA block
```markdown
#!qa
What is the process when water turns
from liquid to vapor?
::
Evaporation
#!
```

- `#!qa` starts the QA block.
- `::` on its own line separates the question from the answer.
- `#!` ends the QA block.

### Multi-line cloze block
```markdown
#!cloze
Example question text
content that is on {{answer}} another line
#!
```

For multiple blanks, map answers explicitly in the answer section:

```markdown
#!cloze
Example question text
content that is on {{answer1}} another line {{answer2}}
::
answer1:
```
multiple line answer 1 option 1
continued line
```
answer2:singlelineoption1|*answer*
#!
```

### Multi-line multiple-choice block
Use natural markdown list syntax for options.

```markdown
#!mcq
Example question text
question continued
::
- [ ] option 1
- [x] option 2 that is the answer
  continued line
- [ ] option 3
#!
```

- `- [ ]` marks an incorrect option.
- `- [x]` marks the correct option.
- Indent continuation lines under the option text.

## Inline image support
Images may appear in questions or answers using Obsidian image links, and they render normally because QA content remains plain markdown.

```markdown
What is shown? ![[images/heart.png]]::Heart
```

Or:

```markdown
What organ is shown?::![[images/heart.png]]
```

## Escape rules

If your content needs to include reserved marker lines literally, escape them with a backslash.

- `\#!` for a literal block end marker
- `\::` for a literal separator line
- `\{{` for a literal cloze opening delimiter
- `\}}` for a literal cloze closing delimiter

If single-line text needs literal `|` or `*` characters, use a multi-line block instead.

### Example with escaped markers
```markdown
#!qa
This line contains a literal separator \::
This line contains a literal block end marker \#!
::
Answer text
#!
```

## Author guidance

- Prefer single-line formats for short questions and answers.
- Use multi-line blocks for longer content, embedded images, multiple blanks, or special characters.
- Keep normal note text outside QA blocks.
- Use block markers exactly as shown so the parser can distinguish QA content from normal notes.

## Example file with mixed notes and QA content

```markdown
Here is normal note text in Obsidian.
It can contain links, headings, and inline images.

#!qa
What is shown? ![[images/heart.png]]
::
Heart
#!

More note content here.

#!mcq
Which option is correct?
::
- [ ] Wrong answer
- [x] Right answer
#!
```

## Summary

This markdown format is intended to be:

- easy to author in Obsidian,
- compatible with inline markdown and images,
- clear for a parser to read,
- flexible across simple and complex question types.
