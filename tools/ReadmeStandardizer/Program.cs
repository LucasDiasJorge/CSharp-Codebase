using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

internal static class Program
{
    private static readonly string[] ReadmeNames = ["README.md", "readme.md"];
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".github",
        ".vs",
        "bin",
        "obj"
    };
    private static readonly Regex CommandLineRegex = new(
        "^(git clone|dotnet\\b|docker\\b|cd\\b|sqlcmd\\b|curl\\b|invoke-restmethod\\b|redis-cli\\b|mysql\\b|psql\\b|npm\\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static int Main(string[] args)
    {
        try
        {
            string rootPath = GetRootPath(args);
            bool includeRootReadme = HasFlag(args, "--include-root");
            List<string> readmePaths = GetReadmePaths(rootPath, includeRootReadme);

            foreach (string readmePath in readmePaths)
            {
                StandardizeReadme(readmePath, rootPath);
            }

            Console.WriteLine($"READMEs padronizados em {rootPath}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    private static string GetRootPath(string[] args)
    {
        for (int index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], "--root", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(args[index + 1]);
            }
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    }

    private static bool HasFlag(string[] args, string flag)
    {
        foreach (string argument in args)
        {
            if (string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static List<string> GetReadmePaths(string rootPath, bool includeRootReadme)
    {
        List<string> readmePaths = new List<string>();
        IEnumerable<string> candidates = Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories);

        foreach (string candidate in candidates)
        {
            if (!ReadmeNames.Contains(Path.GetFileName(candidate), StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (IsIgnoredPath(candidate, rootPath))
            {
                continue;
            }

            string normalizedCandidate = Path.GetFullPath(candidate);
            string rootReadmePath = Path.Combine(rootPath, "README.md");

            if (!includeRootReadme && string.Equals(normalizedCandidate, rootReadmePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            readmePaths.Add(candidate);
        }

        readmePaths.Sort(StringComparer.OrdinalIgnoreCase);
        return readmePaths;
    }

    private static bool IsIgnoredPath(string filePath, string rootPath)
    {
        string relativePath = NormalizePath(Path.GetRelativePath(rootPath, filePath));
        string[] segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        for (int index = 0; index < segments.Length - 1; index++)
        {
            if (IgnoredDirectories.Contains(segments[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static void StandardizeReadme(string readmePath, string rootPath)
    {
        string content = SanitizeRawMarkdown(File.ReadAllText(readmePath));
        MarkdownDocument document = ParseMarkdownDocument(content);
        string directoryPath = Path.GetDirectoryName(readmePath) ?? rootPath;
        string relativeDirectory = NormalizePath(Path.GetRelativePath(rootPath, directoryPath));
        string title = string.IsNullOrWhiteSpace(document.Title) ? ToPrettyName(Path.GetFileName(directoryPath)) : document.Title;
        string introContent = NormalizeIntroBlock(document.Intro);
        string summary = GetSummary(CleanupBucketContent("overview", introContent));

        if (string.IsNullOrWhiteSpace(summary))
        {
            string categoryDescription = GetCategoryDescription(relativeDirectory);
            summary = $"Projeto didático do CSharp-101 dedicado a {title}, com foco em {categoryDescription}.";
        }

        Dictionary<string, List<MarkdownSection>> buckets = CreateBuckets();

        foreach (MarkdownSection section in document.Sections)
        {
            string bucketName = GetSectionBucket(section.Heading);
            buckets[bucketName].Add(section);
        }

        bool hasDirectProject = Directory.EnumerateFiles(directoryPath, "*.csproj", SearchOption.TopDirectoryOnly).Any();
        bool hasChildReadmes = Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly)
            .Any(HasLocalReadme);
        bool isCollection = !hasDirectProject && hasChildReadmes;

        string rawOverviewContent = CombineSections(buckets["overview"]);
        string embeddedOverviewDocument = string.Empty;
        if (TryExtractEmbeddedMarkdownDocument(rawOverviewContent, out string extractedOverviewDocument))
        {
            embeddedOverviewDocument = extractedOverviewDocument;
            rawOverviewContent = string.Empty;
        }

        string overviewContent = CleanupBucketContent("overview", rawOverviewContent);
        if (string.IsNullOrWhiteSpace(overviewContent))
        {
            string cleanedIntroContent = CleanupBucketContent("overview", introContent);
            overviewContent = string.IsNullOrWhiteSpace(cleanedIntroContent) ? summary : cleanedIntroContent;
        }

        string conceptsContent = CleanupBucketContent("concepts", CombineSections(buckets["concepts"]));
        if (string.IsNullOrWhiteSpace(conceptsContent))
        {
            conceptsContent = GetGeneratedConcepts(title, relativeDirectory, isCollection);
        }

        string objectivesContent = CleanupBucketContent("objectives", CombineSections(buckets["objectives"]));
        if (string.IsNullOrWhiteSpace(objectivesContent))
        {
            objectivesContent = GetGeneratedObjectives(title, relativeDirectory, isCollection);
        }

        string existingRunContent = CleanupBucketContent("run", CombineSections(buckets["run"]));
        string runContent = BuildRunContent(directoryPath, rootPath, existingRunContent);

        string bestPracticesContent = CleanupBucketContent("best", CombineSections(buckets["best"]));
        if (string.IsNullOrWhiteSpace(bestPracticesContent))
        {
            bestPracticesContent = GetGeneratedBestPractices(isCollection);
        }

        string referencesContent = CleanupBucketContent("references", CombineSections(buckets["references"]));
        string documentationSectionContent = CleanupBucketContent("docs", CombineSections(buckets["docs"]));
        SectionSplit referencesSplit = SplitReferenceDocumentationContent(referencesContent, preferDocumentation: false);
        SectionSplit documentationSplit = SplitReferenceDocumentationContent(documentationSectionContent, preferDocumentation: true);
        referencesContent = CleanupBucketContent(
            "references",
            MergeMarkdownFragments(referencesSplit.ReferenceContent, documentationSplit.ReferenceContent));
        documentationSectionContent = CleanupBucketContent(
            "docs",
            MergeMarkdownFragments(
                documentationSplit.DocumentationContent,
                referencesSplit.DocumentationContent,
                GetDocumentationLinks(directoryPath)));
        string rawRemainingContent = CombineSections(buckets["other"]);
        if (!string.IsNullOrWhiteSpace(embeddedOverviewDocument))
        {
            rawRemainingContent = string.IsNullOrWhiteSpace(rawRemainingContent)
                ? embeddedOverviewDocument
                : embeddedOverviewDocument + Environment.NewLine + Environment.NewLine + rawRemainingContent;
        }

        string remainingContent = DemoteMarkdownHeadings(CleanupBucketContent("other", rawRemainingContent));
        string structureContent = GetDirectoryTreeBlock(directoryPath);

        StringBuilder builder = new StringBuilder();
        AppendSection(builder, $"# {title}");
        AppendSection(builder, "## Visão geral", overviewContent.Trim());
        AppendSection(builder, "## Conceitos abordados", conceptsContent.Trim());
        AppendSection(builder, "## Objetivos de aprendizagem", objectivesContent.Trim());
        AppendSection(builder, "## Estrutura do projeto", structureContent);
        AppendSection(builder, "## Como executar", runContent.Trim());
        AppendSection(builder, "## Boas práticas e pontos de atenção", bestPracticesContent.Trim());

        if (!string.IsNullOrWhiteSpace(remainingContent))
        {
            AppendSection(builder, "## Conteúdo complementar", remainingContent.Trim());
        }

        if (!string.IsNullOrWhiteSpace(referencesContent))
        {
            AppendSection(builder, "## Referências", referencesContent.Trim());
        }

        if (!string.IsNullOrWhiteSpace(documentationSectionContent))
        {
            AppendSection(builder, "## Documentação complementar", documentationSectionContent.Trim());
        }

        string newContent = builder.ToString().TrimEnd() + Environment.NewLine;
        File.WriteAllText(readmePath, newContent, new UTF8Encoding(false));
    }

    private static Dictionary<string, List<MarkdownSection>> CreateBuckets()
    {
        return new Dictionary<string, List<MarkdownSection>>(StringComparer.OrdinalIgnoreCase)
        {
            ["overview"] = new List<MarkdownSection>(),
            ["concepts"] = new List<MarkdownSection>(),
            ["objectives"] = new List<MarkdownSection>(),
            ["structure"] = new List<MarkdownSection>(),
            ["run"] = new List<MarkdownSection>(),
            ["best"] = new List<MarkdownSection>(),
            ["references"] = new List<MarkdownSection>(),
            ["docs"] = new List<MarkdownSection>(),
            ["meta"] = new List<MarkdownSection>(),
            ["other"] = new List<MarkdownSection>()
        };
    }

    private static string SanitizeRawMarkdown(string content)
    {
        string[] lines = Regex.Split(content, "\\r?\\n");
        List<string> sanitizedLines = new List<string>();
        bool inCodeFence = false;

        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];
            string trimmedLine = line.Trim();

            if (!inCodeFence && ShouldIgnoreStandaloneLine(trimmedLine))
            {
                continue;
            }

            if (IsFenceLine(trimmedLine))
            {
                if (!inCodeFence
                    && string.Equals(trimmedLine, "```", StringComparison.Ordinal)
                    && NextNonEmptyLineIsHeading(lines, index + 1))
                {
                    continue;
                }

                inCodeFence = !inCodeFence;
            }

            sanitizedLines.Add(line);
        }

        return string.Join(Environment.NewLine, sanitizedLines);
    }

    private static MarkdownDocument ParseMarkdownDocument(string content)
    {
        string[] lines = Regex.Split(content, "\\r?\\n");
        int sectionHeadingLevel = GetSectionHeadingLevel(lines);
        string title = string.Empty;
        List<string> introLines = new List<string>();
        List<MarkdownSection> sections = new List<MarkdownSection>();
        string? currentHeading = null;
        List<string> currentLines = new List<string>();
        bool inCodeFence = false;
        bool titleFound = false;

        foreach (string line in lines)
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inCodeFence = !inCodeFence;
            }

            if (!inCodeFence && !titleFound)
            {
                Match titleMatch = Regex.Match(line, "^\\s*#\\s+(.+)$");
                if (titleMatch.Success)
                {
                    title = CleanHeading(titleMatch.Groups[1].Value);
                    titleFound = true;
                }

                continue;
            }

            Match sectionMatch = Regex.Match(line, "^\\s*(#{2,6})\\s+(.+)$");
            if (!inCodeFence
                && sectionHeadingLevel > 0
                && sectionMatch.Success
                && sectionMatch.Groups[1].Value.Length == sectionHeadingLevel)
            {
                if (!string.IsNullOrWhiteSpace(currentHeading))
                {
                    sections.Add(new MarkdownSection(currentHeading, NormalizeTextBlock(string.Join(Environment.NewLine, currentLines))));
                }

                currentHeading = CleanHeading(sectionMatch.Groups[2].Value);
                currentLines = new List<string>();
                continue;
            }

            if (string.IsNullOrWhiteSpace(currentHeading))
            {
                introLines.Add(line);
            }
            else
            {
                currentLines.Add(line);
            }
        }

        if (!string.IsNullOrWhiteSpace(currentHeading))
        {
            sections.Add(new MarkdownSection(currentHeading, NormalizeTextBlock(string.Join(Environment.NewLine, currentLines))));
        }

        return new MarkdownDocument(title, NormalizeTextBlock(string.Join(Environment.NewLine, introLines)), sections);
    }

    private static string CleanHeading(string heading)
    {
        string cleanedHeading = heading.Trim();
        cleanedHeading = Regex.Replace(cleanedHeading, "^[^\\p{L}\\p{N}]+", string.Empty);
        cleanedHeading = Regex.Replace(cleanedHeading, "\\s+", " ");
        return cleanedHeading.Trim();
    }

    private static string NormalizeTextBlock(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string[] lines = Regex.Split(text, "\\r?\\n");
        List<string> filteredLines = new List<string>();

        foreach (string line in lines)
        {
            if (ShouldIgnoreStandaloneLine(line))
            {
                continue;
            }

            if (Regex.IsMatch(line, "^\\s*---+\\s*$"))
            {
                continue;
            }

            if (Regex.IsMatch(line, "^\\s*<!--.*-->\\s*$"))
            {
                continue;
            }

            if (Regex.IsMatch(line, "^\\s*!\\[[^\\r\\n]*$"))
            {
                continue;
            }

            if (Regex.IsMatch(line, "^\\s*\\[!\\[[^\\r\\n]*$"))
            {
                continue;
            }

            filteredLines.Add(line);
        }

        return TrimUnmatchedEdgeCodeFences(NormalizeBlankLines(string.Join(Environment.NewLine, filteredLines)));
    }

    private static string NormalizeIntroBlock(string text)
    {
        string normalizedText = NormalizeTextBlock(text);

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return string.Empty;
        }

        string[] lines = Regex.Split(normalizedText, "\\r?\\n");
        List<string> filteredLines = new List<string>();

        foreach (string line in lines)
        {
            if (Regex.IsMatch(line, "^\\s*#\\s+.+$"))
            {
                continue;
            }

            filteredLines.Add(line);
        }

        return string.Join(Environment.NewLine, filteredLines).Trim();
    }

    private static string GetSummary(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string[] paragraphs = Regex.Split(text, "(?:\\r?\\n){2,}");
        foreach (string paragraph in paragraphs)
        {
            if (!string.IsNullOrWhiteSpace(paragraph))
            {
                return paragraph.Trim();
            }
        }

        return string.Empty;
    }

    private static string CleanupBucketContent(string bucketName, string content)
    {
        string cleanedContent = NormalizeTextBlock(content);

        if (string.IsNullOrWhiteSpace(cleanedContent))
        {
            return string.Empty;
        }

        if (string.Equals(bucketName, "overview", StringComparison.OrdinalIgnoreCase))
        {
            cleanedContent = RemoveLeadingNoiseParagraphs(cleanedContent);
        }

        if (string.Equals(bucketName, "concepts", StringComparison.OrdinalIgnoreCase))
        {
            cleanedContent = RemoveLeadingHeading(cleanedContent, "conceitos abordados", "conceitos", "conceitos fundamentais", "conceitos principais", "conceitos importantes");
            cleanedContent = RemoveLeadingHeadingByPattern(cleanedContent, "conceit");
        }

        if (string.Equals(bucketName, "objectives", StringComparison.OrdinalIgnoreCase))
        {
            cleanedContent = RemoveLeadingHeading(cleanedContent, "objetivos de aprendizado", "objetivos de aprendizagem", "objetivo", "o que voce aprendera", "o que voce vai aprender");
            cleanedContent = RemoveLeadingHeadingByPattern(cleanedContent, "objetiv");
        }

        if (string.Equals(bucketName, "run", StringComparison.OrdinalIgnoreCase))
        {
            cleanedContent = RemoveLeadingHeading(cleanedContent, "como executar", "como usar", "execucao", "executar", "running");
        }

        if (string.Equals(bucketName, "best", StringComparison.OrdinalIgnoreCase))
        {
            cleanedContent = RemoveLeadingHeading(cleanedContent, "boas praticas", "boas praticas demonstradas", "boas praticas aplicadas", "pontos de atencao");
            cleanedContent = RemoveLeadingHeadingByPattern(cleanedContent, "boas praticas|pontos de atencao");
        }

        if (string.Equals(bucketName, "references", StringComparison.OrdinalIgnoreCase))
        {
            cleanedContent = RemoveLeadingHeading(cleanedContent, "referencias", "recursos adicionais", "recursos online", "livros complementares");
            cleanedContent = RemoveLeadingHeadingByPattern(cleanedContent, "referenc");
        }

        if (string.Equals(bucketName, "docs", StringComparison.OrdinalIgnoreCase))
        {
            cleanedContent = RemoveLeadingHeading(cleanedContent, "documentacao complementar");
            cleanedContent = DeduplicateDocumentationLinkLines(cleanedContent);
        }

        return NormalizeBlankLines(cleanedContent);
    }

    private static string RemoveLeadingHeading(string content, params string[] candidateHeadings)
    {
        List<string> lines = Regex.Split(content, "\\r?\\n").ToList();
        bool shouldContinue = true;

        while (shouldContinue)
        {
            shouldContinue = false;
            int firstContentLineIndex = -1;

            for (int index = 0; index < lines.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(lines[index]))
                {
                    firstContentLineIndex = index;
                    break;
                }
            }

            if (firstContentLineIndex < 0)
            {
                break;
            }

            Match headingMatch = Regex.Match(lines[firstContentLineIndex], "^\\s*#{3,6}\\s+(.+)$");
            if (!headingMatch.Success)
            {
                break;
            }

            string normalizedHeading = NormalizeHeadingKey(headingMatch.Groups[1].Value);
            foreach (string candidateHeading in candidateHeadings)
            {
                if (!string.Equals(normalizedHeading, NormalizeHeadingKey(candidateHeading), StringComparison.Ordinal))
                {
                    continue;
                }

                lines.RemoveAt(firstContentLineIndex);
                if (firstContentLineIndex < lines.Count && string.IsNullOrWhiteSpace(lines[firstContentLineIndex]))
                {
                    lines.RemoveAt(firstContentLineIndex);
                }

                shouldContinue = true;
                break;
            }
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    private static string RemoveLeadingHeadingByPattern(string content, string headingPattern)
    {
        List<string> lines = Regex.Split(content, "\\r?\\n").ToList();
        bool shouldContinue = true;

        while (shouldContinue)
        {
            shouldContinue = false;
            int firstContentLineIndex = -1;

            for (int index = 0; index < lines.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(lines[index]))
                {
                    firstContentLineIndex = index;
                    break;
                }
            }

            if (firstContentLineIndex < 0)
            {
                break;
            }

            Match headingMatch = Regex.Match(lines[firstContentLineIndex], "^\\s*#{3,6}\\s+(.+)$");
            if (!headingMatch.Success)
            {
                break;
            }

            string normalizedHeading = NormalizeHeadingKey(headingMatch.Groups[1].Value);
            if (!Regex.IsMatch(normalizedHeading, headingPattern, RegexOptions.CultureInvariant))
            {
                break;
            }

            lines.RemoveAt(firstContentLineIndex);
            if (firstContentLineIndex < lines.Count && string.IsNullOrWhiteSpace(lines[firstContentLineIndex]))
            {
                lines.RemoveAt(firstContentLineIndex);
            }

            shouldContinue = true;
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    private static string RemoveLeadingNoiseParagraphs(string content)
    {
        string[] paragraphs = Regex.Split(content.Trim(), "(?:\\r?\\n){2,}");
        int startIndex = 0;

        while (startIndex < paragraphs.Length && IsNoiseParagraph(paragraphs[startIndex]))
        {
            startIndex++;
        }

        if (startIndex >= paragraphs.Length)
        {
            return string.Empty;
        }

        List<string> keptParagraphs = new List<string>();
        for (int index = startIndex; index < paragraphs.Length; index++)
        {
            keptParagraphs.Add(paragraphs[index].Trim());
        }

        return string.Join(Environment.NewLine + Environment.NewLine, keptParagraphs).Trim();
    }

    private static bool IsNoiseParagraph(string paragraph)
    {
        string trimmedParagraph = paragraph.Trim();

        if (string.IsNullOrWhiteSpace(trimmedParagraph))
        {
            return true;
        }

        if (trimmedParagraph.StartsWith("```markdown", StringComparison.OrdinalIgnoreCase)
            || trimmedParagraph.StartsWith("```md", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string[] lines = Regex.Split(trimmedParagraph, "\\r?\\n");
        bool hasMeaningfulLine = false;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            if (trimmedLine.StartsWith("<!--", StringComparison.Ordinal)
                || trimmedLine.StartsWith("[![", StringComparison.Ordinal)
                || trimmedLine.StartsWith("```", StringComparison.Ordinal)
                || CommandLineRegex.IsMatch(trimmedLine))
            {
                continue;
            }

            hasMeaningfulLine = true;
            break;
        }

        return !hasMeaningfulLine;
    }

    private static bool TryExtractEmbeddedMarkdownDocument(string content, out string extractedDocument)
    {
        string trimmedContent = content.Trim();
        extractedDocument = string.Empty;

        if (!(trimmedContent.StartsWith("```markdown", StringComparison.OrdinalIgnoreCase)
            || trimmedContent.StartsWith("```md", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        int firstLineBreakIndex = trimmedContent.IndexOf('\n');
        int lastFenceIndex = trimmedContent.LastIndexOf("```", StringComparison.Ordinal);
        if (firstLineBreakIndex < 0 || lastFenceIndex <= firstLineBreakIndex)
        {
            return false;
        }

        string innerContent = trimmedContent.Substring(firstLineBreakIndex + 1, lastFenceIndex - firstLineBreakIndex - 1).Trim();
        if (!Regex.IsMatch(innerContent, "^\\s*##?\\s+", RegexOptions.Multiline))
        {
            return false;
        }

        extractedDocument = NormalizeTextBlock(innerContent);
        return !string.IsNullOrWhiteSpace(extractedDocument);
    }

    private static string BuildRunContent(string directoryPath, string rootPath, string existingRunContent)
    {
        string generatedRunContent = GetGeneratedRunContent(directoryPath, rootPath);

        if (string.IsNullOrWhiteSpace(existingRunContent))
        {
            return generatedRunContent;
        }

        if (IsGenericRunContent(generatedRunContent))
        {
            return existingRunContent;
        }

        string supplementalNotes = GetSupplementalRunNotes(existingRunContent);
        if (string.IsNullOrWhiteSpace(supplementalNotes))
        {
            return generatedRunContent;
        }

        return generatedRunContent.TrimEnd() + Environment.NewLine + Environment.NewLine + supplementalNotes;
    }

    private static bool IsGenericRunContent(string runContent)
    {
        return runContent.StartsWith("Consulte o código desta pasta", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetSupplementalRunNotes(string runContent)
    {
        string contentWithoutCodeBlocks = Regex.Replace(runContent, "```[\\s\\S]*?```", string.Empty);
        string[] lines = Regex.Split(contentWithoutCodeBlocks, "\\r?\\n");
        List<string> keptLines = new List<string>();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                keptLines.Add(string.Empty);
                continue;
            }

            if (trimmedLine.StartsWith("<!--", StringComparison.Ordinal)
                || CommandLineRegex.IsMatch(trimmedLine))
            {
                continue;
            }

            if (IsGeneratedRunLine(trimmedLine))
            {
                continue;
            }

            Match headingMatch = Regex.Match(trimmedLine, "^#{3,6}\\s+(.+)$");
            if (headingMatch.Success && Regex.IsMatch(NormalizeHeadingKey(headingMatch.Groups[1].Value), "como executar|como usar|execucao|executar"))
            {
                continue;
            }

            keptLines.Add(line);
        }

        return NormalizeBlankLines(string.Join(Environment.NewLine, keptLines));
    }

    private static SectionSplit SplitReferenceDocumentationContent(string content, bool preferDocumentation)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new SectionSplit(string.Empty, string.Empty);
        }

        string[] lines = Regex.Split(content, "\\r?\\n");
        List<string> referenceLines = new List<string>();
        List<string> documentationLines = new List<string>();
        List<string> currentTarget = preferDocumentation ? documentationLines : referenceLines;
        bool inCodeFence = false;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (IsFenceLine(trimmedLine))
            {
                inCodeFence = !inCodeFence;
                currentTarget.Add(line);
                continue;
            }

            if (!inCodeFence)
            {
                Match headingMatch = Regex.Match(line, "^\\s*#{3,6}\\s+(.+)$");
                if (headingMatch.Success)
                {
                    string normalizedHeading = NormalizeHeadingKey(headingMatch.Groups[1].Value);
                    if (Regex.IsMatch(normalizedHeading, "documentacao complementar"))
                    {
                        currentTarget = documentationLines;
                        continue;
                    }

                    if (Regex.IsMatch(normalizedHeading, "referenc"))
                    {
                        currentTarget = referenceLines;
                        continue;
                    }
                }
            }

            currentTarget.Add(line);
        }

        return new SectionSplit(
            NormalizeBlankLines(string.Join(Environment.NewLine, referenceLines)),
            DeduplicateDocumentationLinkLines(NormalizeBlankLines(string.Join(Environment.NewLine, documentationLines))));
    }

    private static string MergeMarkdownFragments(params string[] fragments)
    {
        List<string> mergedBlocks = new List<string>();
        HashSet<string> seenBlocks = new HashSet<string>(StringComparer.Ordinal);

        foreach (string fragment in fragments)
        {
            if (string.IsNullOrWhiteSpace(fragment))
            {
                continue;
            }

            string[] blocks = Regex.Split(NormalizeBlankLines(fragment), "(?:\\r?\\n){2,}");
            foreach (string block in blocks)
            {
                string trimmedBlock = block.Trim();
                if (string.IsNullOrWhiteSpace(trimmedBlock))
                {
                    continue;
                }

                if (!seenBlocks.Add(trimmedBlock))
                {
                    continue;
                }

                mergedBlocks.Add(trimmedBlock);
            }
        }

        return string.Join(Environment.NewLine + Environment.NewLine, mergedBlocks);
    }

    private static string DeduplicateDocumentationLinkLines(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        string[] lines = Regex.Split(content, "\\r?\\n");
        List<string> filteredLines = new List<string>();
        HashSet<string> seenLinkTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string line in lines)
        {
            if (TryGetDocumentationLinkTarget(line, out string linkTarget))
            {
                if (!seenLinkTargets.Add(linkTarget))
                {
                    continue;
                }
            }

            filteredLines.Add(line);
        }

        return NormalizeBlankLines(string.Join(Environment.NewLine, filteredLines));
    }

    private static bool TryGetDocumentationLinkTarget(string line, out string linkTarget)
    {
        Match match = Regex.Match(line, "^\\s*[-*+]\\s+\\[[^\\]]+\\]\\(([^)]+\\.md)\\)");
        if (match.Success)
        {
            linkTarget = match.Groups[1].Value.Trim();
            return true;
        }

        linkTarget = string.Empty;
        return false;
    }

    private static string CombineSections(List<MarkdownSection> sections)
    {
        if (sections.Count == 0)
        {
            return string.Empty;
        }

        if (sections.Count == 1)
        {
            return sections[0].Content.Trim();
        }

        StringBuilder builder = new StringBuilder();
        bool isFirstSection = true;

        foreach (MarkdownSection section in sections)
        {
            if (string.IsNullOrWhiteSpace(section.Content))
            {
                continue;
            }

            if (!isFirstSection)
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            builder.AppendLine($"### {section.Heading}");
            builder.AppendLine();
            builder.Append(section.Content.Trim());
            isFirstSection = false;
        }

        return builder.ToString().Trim();
    }

    private static string GetSectionBucket(string heading)
    {
        string normalizedHeading = NormalizeHeadingKey(heading);

        if (Regex.IsMatch(normalizedHeading, "visao geral|o que e|o que sao|este projeto"))
        {
            return "overview";
        }

        if (Regex.IsMatch(normalizedHeading, "conceit|problema resolvido|conceitos fundamentais|conceitos principais|conceitos importantes"))
        {
            return "concepts";
        }

        if (Regex.IsMatch(normalizedHeading, "objetiv|voce aprendera|aprendera"))
        {
            return "objectives";
        }

        if (Regex.IsMatch(normalizedHeading, "estrutura|estrutura do projeto|estrutura de pastas|estrutura principal|estrutura relevante|organizacao do projeto"))
        {
            return "structure";
        }

        if (Regex.IsMatch(normalizedHeading, "como executar|como usar|como executar os projetos|execucao|executar|setup"))
        {
            return "run";
        }

        if (Regex.IsMatch(normalizedHeading, "boas praticas|pontos de atencao"))
        {
            return "best";
        }

        if (Regex.IsMatch(normalizedHeading, "documentacao complementar"))
        {
            return "docs";
        }

        if (Regex.IsMatch(normalizedHeading, "referenc|recursos"))
        {
            return "references";
        }

        if (Regex.IsMatch(normalizedHeading, "indice|sumario|checklist"))
        {
            return "meta";
        }

        return "other";
    }

    private static int GetSectionHeadingLevel(string[] lines)
    {
        bool inCodeFence = false;
        bool titleFound = false;
        List<int> headingLevels = new List<int>();

        foreach (string line in lines)
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inCodeFence = !inCodeFence;
                continue;
            }

            if (!inCodeFence && !titleFound)
            {
                if (Regex.IsMatch(line, "^\\s*#\\s+.+$"))
                {
                    titleFound = true;
                }

                continue;
            }

            if (inCodeFence || !titleFound)
            {
                continue;
            }

            Match headingMatch = Regex.Match(line, "^\\s*(#{2,6})\\s+.+$");
            if (headingMatch.Success)
            {
                headingLevels.Add(headingMatch.Groups[1].Value.Length);
            }
        }

        if (headingLevels.Contains(2))
        {
            return 2;
        }

        if (headingLevels.Contains(3))
        {
            return 3;
        }

        return headingLevels.Count == 0 ? 0 : headingLevels.Min();
    }

    private static string NormalizeBlankLines(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return Regex.Replace(text.Trim(), "(?:\\r?\\n){3,}", Environment.NewLine + Environment.NewLine);
    }

    private static string DemoteMarkdownHeadings(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        string[] lines = Regex.Split(content, "\\r?\\n");
        bool inCodeFence = false;
        List<string> rewrittenLines = new List<string>();

        foreach (string line in lines)
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inCodeFence = !inCodeFence;
                rewrittenLines.Add(line);
                continue;
            }

            if (inCodeFence)
            {
                rewrittenLines.Add(line);
                continue;
            }

            Match headingMatch = Regex.Match(line, "^(\\s*)(#{1,6})(\\s+.*)$");
            if (!headingMatch.Success)
            {
                rewrittenLines.Add(line);
                continue;
            }

            string indentation = headingMatch.Groups[1].Value;
            string headingHashes = headingMatch.Groups[2].Value;
            string headingSuffix = headingMatch.Groups[3].Value;
            string demotedHashes = headingHashes.Length switch
            {
                <= 2 => "###",
                < 6 => headingHashes + "#",
                _ => headingHashes
            };
            rewrittenLines.Add(indentation + demotedHashes + headingSuffix);
        }

        return string.Join(Environment.NewLine, rewrittenLines).Trim();
    }

    private static string NormalizeHeadingKey(string heading)
    {
        string normalizedHeading = RemoveDiacritics(heading).ToLowerInvariant();
        normalizedHeading = Regex.Replace(normalizedHeading, "[^a-z0-9 ]", " ");
        normalizedHeading = Regex.Replace(normalizedHeading, "\\s+", " ").Trim();
        return normalizedHeading;
    }

    private static string GetGeneratedConcepts(string title, string relativeDirectory, bool isCollection)
    {
        string categoryDescription = GetCategoryDescription(relativeDirectory);
        string[] lines = isCollection
            ?
            [
                $"- Organização de exemplos sobre {categoryDescription}.",
                $"- Navegação entre implementações relacionadas a {title}.",
                "- Comparação prática entre abordagens presentes nesta pasta."
            ]
            :
            [
                $"- Exemplo didático sobre {title} no contexto de {categoryDescription}.",
                "- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.",
                "- Observação prática das decisões técnicas presentes nesta implementação."
            ];

        return string.Join(Environment.NewLine, lines);
    }

    private static string GetGeneratedObjectives(string title, string relativeDirectory, bool isCollection)
    {
        string categoryDescription = GetCategoryDescription(relativeDirectory);
        string[] lines = isCollection
            ?
            [
                $"- Identificar como os exemplos desta pasta cobrem {categoryDescription}.",
                "- Escolher o subprojeto mais adequado para aprofundar o estudo.",
                "- Reutilizar a navegação da pasta como índice prático de consulta."
            ]
            :
            [
                $"- Entender como {title} se aplica em um cenário prático de {categoryDescription}.",
                "- Executar o exemplo com comandos direcionados ao projeto correto.",
                "- Usar a pasta como referência rápida para estudo e revisão posterior."
            ];

        return string.Join(Environment.NewLine, lines);
    }

    private static string GetGeneratedBestPractices(bool isCollection)
    {
        string[] lines = isCollection
            ?
            [
                "- Use os READMEs dos subprojetos como ponto de entrada antes de alterar o código.",
                "- Compare implementações relacionadas mantendo o mesmo conceito em foco.",
                "- Prefira build ou execução direcionada por projeto em vez de validar a solução inteira."
            ]
            :
            [
                "- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.",
                "- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.",
                "- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais."
            ];

        return string.Join(Environment.NewLine, lines);
    }

    private static string GetGeneratedRunContent(string directoryPath, string rootPath)
    {
        List<string> directProjects = Directory.EnumerateFiles(directoryPath, "*.csproj", SearchOption.TopDirectoryOnly)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (directProjects.Count > 0)
        {
            string mainProject = SelectMainProject(directProjects, directoryPath);
            return GetProjectCommandBlock(mainProject, rootPath);
        }

        List<string> descendantProjects = Directory.EnumerateFiles(directoryPath, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !ContainsIgnoredBuildSegment(path))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        List<string> nonTestProjects = descendantProjects
            .Where(path => !IsTestProject(path))
            .Take(6)
            .ToList();

        if (nonTestProjects.Count > 0)
        {
            List<string> lines = new List<string>
            {
                "Escolha um dos projetos abaixo para execução direcionada:",
                string.Empty
            };

            foreach (string projectPath in nonTestProjects)
            {
                string relativeProjectPath = NormalizePath(Path.GetRelativePath(rootPath, projectPath));
                string projectDirectory = Path.GetDirectoryName(projectPath) ?? directoryPath;
                string verb = File.Exists(Path.Combine(projectDirectory, "Program.cs")) ? "run --project" : "build";
                lines.Add($"- `dotnet {verb} {relativeProjectPath}`");
            }

            return string.Join(Environment.NewLine, lines);
        }

        List<string> childReadmes = Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly)
            .Where(HasLocalReadme)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        if (childReadmes.Count > 0)
        {
            List<string> lines = new List<string>
            {
                "Esta pasta organiza subprojetos. Comece por um dos READMEs abaixo:",
                string.Empty
            };

            foreach (string childDirectory in childReadmes)
            {
                string readmePath = GetLocalReadmePath(childDirectory);
                string relativeReadmePath = NormalizePath(Path.GetRelativePath(directoryPath, readmePath));
                lines.Add($"- [{Path.GetFileName(childDirectory)}]({relativeReadmePath})");
            }

            return string.Join(Environment.NewLine, lines);
        }

        return "Consulte o código desta pasta e os projetos relacionados antes de executar comandos específicos.";
    }

    private static string SelectMainProject(List<string> projectPaths, string directoryPath)
    {
        string folderName = Path.GetFileName(directoryPath);

        foreach (string projectPath in projectPaths)
        {
            string projectName = Path.GetFileNameWithoutExtension(projectPath);
            if (string.Equals(projectName, folderName, StringComparison.OrdinalIgnoreCase))
            {
                return projectPath;
            }
        }

        foreach (string projectPath in projectPaths)
        {
            if (!IsTestProject(projectPath))
            {
                return projectPath;
            }
        }

        return projectPaths[0];
    }

    private static bool IsTestProject(string projectPath)
    {
        string projectName = Path.GetFileNameWithoutExtension(projectPath);
        return projectName.EndsWith(".Test", StringComparison.OrdinalIgnoreCase)
            || projectName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase)
            || projectName.EndsWith("Tests", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsIgnoredBuildSegment(string path)
    {
        string normalizedPath = NormalizePath(path);
        string[] segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (string segment in segments)
        {
            if (string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasLocalReadme(string directoryPath)
    {
        return File.Exists(Path.Combine(directoryPath, "README.md"))
            || File.Exists(Path.Combine(directoryPath, "readme.md"));
    }

    private static string GetLocalReadmePath(string directoryPath)
    {
        string uppercaseReadmePath = Path.Combine(directoryPath, "README.md");
        if (File.Exists(uppercaseReadmePath))
        {
            return uppercaseReadmePath;
        }

        return Path.Combine(directoryPath, "readme.md");
    }

    private static string GetProjectCommandBlock(string projectPath, string rootPath)
    {
        string relativeProjectPath = NormalizePath(Path.GetRelativePath(rootPath, projectPath));
        string projectDirectory = Path.GetDirectoryName(projectPath) ?? rootPath;

        if (IsTestProject(projectPath))
        {
            return string.Join(Environment.NewLine, ["```bash", $"dotnet test {relativeProjectPath}", "```"]);
        }

        if (File.Exists(Path.Combine(projectDirectory, "Program.cs")))
        {
            return string.Join(Environment.NewLine, ["```bash", $"dotnet run --project {relativeProjectPath}", "```"]);
        }

        return string.Join(Environment.NewLine, ["```bash", $"dotnet build {relativeProjectPath}", "```"]);
    }

    private static string GetDirectoryTreeBlock(string directoryPath)
    {
        List<string> lines = new List<string> { Path.GetFileName(directoryPath) + "/" };
        AddTreeLines(directoryPath, string.Empty, 0, 2, 8, lines);
        return string.Join(Environment.NewLine, ["```text", string.Join(Environment.NewLine, lines), "```"]);
    }

    private static void AddTreeLines(
        string currentPath,
        string prefix,
        int depth,
        int maxDepth,
        int maxItemsPerLevel,
        List<string> lines)
    {
        if (depth >= maxDepth)
        {
            return;
        }

        List<string> children = Directory.EnumerateFileSystemEntries(currentPath, "*", SearchOption.TopDirectoryOnly)
            .Where(path => !ShouldSkipTreeEntry(path))
            .OrderBy(static path => Directory.Exists(path) ? 0 : 1)
            .ThenBy(static path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToList();

        bool hasMoreItems = children.Count > maxItemsPerLevel;
        List<string> visibleChildren = hasMoreItems ? children.Take(maxItemsPerLevel).ToList() : children;

        for (int index = 0; index < visibleChildren.Count; index++)
        {
            string childPath = visibleChildren[index];
            bool isLastVisible = index == visibleChildren.Count - 1 && !hasMoreItems;
            string branch = isLastVisible ? "\\-- " : "+-- ";
            string name = Path.GetFileName(childPath) + (Directory.Exists(childPath) ? "/" : string.Empty);
            lines.Add(prefix + branch + name);

            if (Directory.Exists(childPath))
            {
                string nextPrefix = prefix + (isLastVisible ? "    " : "|   ");
                AddTreeLines(childPath, nextPrefix, depth + 1, maxDepth, maxItemsPerLevel, lines);
            }
        }

        if (hasMoreItems)
        {
            lines.Add(prefix + "\\-- ...");
        }
    }

    private static bool ShouldSkipTreeEntry(string path)
    {
        string name = Path.GetFileName(path);

        if (ReadmeNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        if (Directory.Exists(path) && IgnoredDirectories.Contains(name))
        {
            return true;
        }

        return false;
    }

    private static string GetDocumentationLinks(string directoryPath)
    {
        List<string> markdownFiles = Directory.EnumerateFiles(directoryPath, "*.md", SearchOption.TopDirectoryOnly)
            .Where(path => !ReadmeNames.Contains(Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (markdownFiles.Count == 0)
        {
            return string.Empty;
        }

        List<string> lines = new List<string>();
        foreach (string markdownFile in markdownFiles)
        {
            string fileName = Path.GetFileName(markdownFile);
            string description = GetMarkdownDescription(markdownFile);
            lines.Add($"- [{fileName}](./{fileName}) - {description}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string GetMarkdownDescription(string filePath)
    {
        IEnumerable<string> lines = File.ReadLines(filePath).Take(20);

        foreach (string line in lines)
        {
            Match titleMatch = Regex.Match(line, "^\\s*#\\s+(.+)$");
            if (titleMatch.Success)
            {
                return CleanHeading(titleMatch.Groups[1].Value);
            }

            if (!string.IsNullOrWhiteSpace(line)
                && !Regex.IsMatch(line, "^\\s*[\\-_*`>]" )
                && !Regex.IsMatch(line, "^\\s*!\\["))
            {
                return line.Trim();
            }
        }

        return "Guia complementar desta pasta.";
    }

    private static string GetCategoryDescription(string relativeDirectory)
    {
        string firstSegment = relativeDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        return firstSegment switch
        {
            "01-Fundamentals" => "conceitos fundamentais da linguagem C# e orientação a objetos",
            "02-AsyncAndConcurrency" => "assincronia, tasks, threads e coordenação de trabalho",
            "03-WebAPIs" => "ASP.NET Core, contratos HTTP e pipeline web",
            "04-Authentication" => "autenticação, autorização e segurança",
            "05-Messaging" => "mensageria, filas, eventos e integração assíncrona",
            "06-Caching" => "estratégias de cache e integração com Redis",
            "07-DesignPatterns" => "design patterns, modelagem OO e código limpo",
            "08-ArchitecturalPatterns" => "padrões arquiteturais e organização de casos de uso",
            "09-Data" => "persistência, bancos de dados e acesso a dados",
            "10-Algorithms" => "algoritmos, estruturas de dados e análise de cenários",
            "11-Utilities" => "utilitários, transformação de dados e observabilidade",
            "12-Testing" => "testes, benchmarks e validação de comportamento",
            "13-SDKsAndLibraries" => "SDKs, bibliotecas e reaproveitamento de código",
            _ => "exemplos didáticos em C# e .NET"
        };
    }

    private static void AppendSection(StringBuilder builder, string heading)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine(heading);
    }

    private static void AppendSection(StringBuilder builder, string heading, string content)
    {
        AppendSection(builder, heading);
        builder.AppendLine();
        builder.AppendLine(content.TrimEnd());
    }

    private static string ToPrettyName(string name)
    {
        string prettyName = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1 $2");
        prettyName = prettyName.Replace('-', ' ').Replace('_', ' ');
        prettyName = Regex.Replace(prettyName, "\\s+", " ");
        return prettyName.Trim();
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string RemoveDiacritics(string text)
    {
        string normalizedText = text.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder();

        foreach (char character in normalizedText)
        {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool ShouldIgnoreStandaloneLine(string line)
    {
        string normalizedLine = NormalizeHeadingKey(line);
        return Regex.IsMatch(normalizedLine, "readme gerado a partir do template comum|readme padronizado|versao condensada|material versao condensada|padronizada com demais projetos|template comum")
            || string.IsNullOrWhiteSpace(normalizedLine) is false && Regex.IsMatch(normalizedLine, "^bons estudos$");
    }

    private static string TrimUnmatchedEdgeCodeFences(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        List<string> lines = Regex.Split(content, "\\r?\\n").ToList();

        while (CountFenceLines(lines) % 2 != 0)
        {
            if (TryRemoveEdgeFenceLine(lines, removeFromEnd: true))
            {
                continue;
            }

            if (TryRemoveEdgeFenceLine(lines, removeFromEnd: false))
            {
                continue;
            }

            break;
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    private static int CountFenceLines(List<string> lines)
    {
        int fenceCount = 0;
        foreach (string line in lines)
        {
            if (IsFenceLine(line.Trim()))
            {
                fenceCount++;
            }
        }

        return fenceCount;
    }

    private static bool TryRemoveEdgeFenceLine(List<string> lines, bool removeFromEnd)
    {
        int index = removeFromEnd ? lines.Count - 1 : 0;
        int step = removeFromEnd ? -1 : 1;

        while (index >= 0 && index < lines.Count)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
            {
                index += step;
                continue;
            }

            if (IsFenceLine(lines[index].Trim()))
            {
                lines.RemoveAt(index);
                return true;
            }

            return false;
        }

        return false;
    }

    private static bool IsFenceLine(string line)
    {
        return line.TrimStart().StartsWith("```", StringComparison.Ordinal);
    }

    private static bool NextNonEmptyLineIsHeading(string[] lines, int startIndex)
    {
        for (int index = startIndex; index < lines.Length; index++)
        {
            string line = lines[index];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            return Regex.IsMatch(line, "^\\s*#{1,2}\\s+.+$");
        }

        return false;
    }

    private static bool IsGeneratedRunLine(string trimmedLine)
    {
        string normalizedLine = NormalizeHeadingKey(trimmedLine);
        if (string.Equals(normalizedLine, "escolha um dos projetos abaixo para execucao direcionada", StringComparison.Ordinal)
            || string.Equals(normalizedLine, "esta pasta organiza subprojetos comece por um dos readmes abaixo", StringComparison.Ordinal)
            || string.Equals(normalizedLine, "consulte o codigo desta pasta e os projetos relacionados antes de executar comandos especificos", StringComparison.Ordinal))
        {
            return true;
        }

        string withoutListMarker = Regex.Replace(trimmedLine, "^[-*+]\\s+", string.Empty).Trim();
        withoutListMarker = withoutListMarker.Trim('`').Trim();
        return CommandLineRegex.IsMatch(withoutListMarker);
    }

    private sealed record MarkdownDocument(string Title, string Intro, List<MarkdownSection> Sections);

    private sealed record MarkdownSection(string Heading, string Content);

    private sealed record SectionSplit(string ReferenceContent, string DocumentationContent);
}