using System;
using System.Collections.Generic;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Parsing;

public interface ICardParser
{
    IReadOnlyList<Card> Parse(Document document);
}

public class CardParser : ICardParser
{
    private readonly ISourceReader _sourceReader;
    private readonly IMarkdownScanner _scanner;
    private readonly IMarkdownSyntaxParser _syntaxParser;
    private readonly IAstBuilder _astBuilder;
    private readonly ISemanticBinder _binder;
    private readonly ISemanticValidator _validator;
    private readonly ICardLowerer _lowerer;
    private readonly ICardEmitter _emitter;

    public CardParser(
        ISourceReader sourceReader,
        IMarkdownScanner scanner,
        IMarkdownSyntaxParser syntaxParser,
        IAstBuilder astBuilder,
        ISemanticBinder binder,
        ISemanticValidator validator,
        ICardLowerer lowerer,
        ICardEmitter emitter)
    {
        _sourceReader = sourceReader ?? throw new ArgumentNullException(nameof(sourceReader));
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        _syntaxParser = syntaxParser ?? throw new ArgumentNullException(nameof(syntaxParser));
        _astBuilder = astBuilder ?? throw new ArgumentNullException(nameof(astBuilder));
        _binder = binder ?? throw new ArgumentNullException(nameof(binder));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _lowerer = lowerer ?? throw new ArgumentNullException(nameof(lowerer));
        _emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
    }

    public CardParser()
    {
        var groupParser = new GroupParser();
        var metadataParser = new MetadataParser(groupParser);
        var utilities = new CardParserUtilities();

        _sourceReader = new SourceReader();
        _scanner = new MarkdownScanner();
        _syntaxParser = new MarkdownSyntaxParser();
        _astBuilder = new AstBuilder();
        _binder = new SemanticBinder(metadataParser);
        _validator = new SemanticValidator(utilities);
        _lowerer = new CardLowerer(utilities);
        _emitter = new CardEmitter();
    }

    public IReadOnlyList<Card> Parse(Document document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        var source = _sourceReader.Read(document.Content);
        var tokens = _scanner.Scan(source);
        var syntax = _syntaxParser.Parse(tokens);
        var ast = _astBuilder.Build(syntax);
        var bound = _binder.Bind(document, ast);
        var diagnostics = _validator.Validate(bound);
        if (diagnostics.HasErrors)
        {
            return Array.Empty<Card>();
        }

        var lowered = _lowerer.Lower(bound);
        return _emitter.Emit(document, lowered);
    }
}
