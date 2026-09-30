namespace Tesserae.Themes.Curiosity
{
    /// <summary>
    /// Every 4 by 4 pixel glyph the Curiosity website draws, by what it is. Generated from the website's own
    /// figures (site/figures.js and the landing page's pixel plays); see <see cref="PixelGlyph"/>.
    /// <list type="bullet">
    /// <item><description><c>Value*</c> - the brand's eight value glyphs.</description></item>
    /// <item><description><c>UseCase*</c> - the five use cases on the landing page's solutions row.</description></item>
    /// <item><description><c>Capability*</c> - the Studio page's six capabilities, in the deep blue.</description></item>
    /// <item><description><c>Category*</c> - the nine integration directory categories.</description></item>
    /// <item><description><c>Play*</c> - the landing page's three problem stories, played once.</description></item>
    /// <item><description><c>Loop*</c> - the 54 looping glyphs on the developer capability pages.</description></item>
    /// </list>
    /// The static ones (Value, UseCase, Capability, Category) animate by building themselves in, the same way a
    /// custom glyph does.
    /// </summary>
    public enum PixelGlyphKind
    {
        /// <summary>Value glyph "connected": ink cells and one Signal cell.</summary>
        ValueConnected,
        /// <summary>Value glyph "resolved": ink cells and one Signal cell.</summary>
        ValueResolved,
        /// <summary>Value glyph "traceable": ink cells and one Signal cell.</summary>
        ValueTraceable,
        /// <summary>Value glyph "sovereign": ink cells and one Signal cell.</summary>
        ValueSovereign,
        /// <summary>Value glyph "fast": ink cells and one Signal cell.</summary>
        ValueFast,
        /// <summary>Value glyph "open": ink cells and one Signal cell.</summary>
        ValueOpen,
        /// <summary>Value glyph "precise": ink cells and one Signal cell.</summary>
        ValuePrecise,
        /// <summary>Value glyph "curious": ink cells and one Signal cell.</summary>
        ValueCurious,
        /// <summary>Use case glyph "knowledge-management", its point in the Signal cell.</summary>
        UseCaseKnowledgeManagement,
        /// <summary>Use case glyph "technical-customer-support", its point in the Signal cell.</summary>
        UseCaseTechnicalCustomerSupport,
        /// <summary>Use case glyph "engineering", its point in the Signal cell.</summary>
        UseCaseEngineering,
        /// <summary>Use case glyph "quality-management", its point in the Signal cell.</summary>
        UseCaseQualityManagement,
        /// <summary>Use case glyph "contracts-and-compliance", its point in the Signal cell.</summary>
        UseCaseContractsAndCompliance,
        /// <summary>Studio capability glyph "graph", every cell in the deep blue.</summary>
        CapabilityGraph,
        /// <summary>Studio capability glyph "retrieval", every cell in the deep blue.</summary>
        CapabilityRetrieval,
        /// <summary>Studio capability glyph "permissions", every cell in the deep blue.</summary>
        CapabilityPermissions,
        /// <summary>Studio capability glyph "models", every cell in the deep blue.</summary>
        CapabilityModels,
        /// <summary>Studio capability glyph "connectors", every cell in the deep blue.</summary>
        CapabilityConnectors,
        /// <summary>Studio capability glyph "memory", every cell in the deep blue.</summary>
        CapabilityMemory,
        /// <summary>Integration category glyph "communication", in the text colour.</summary>
        CategoryCommunication,
        /// <summary>Integration category glyph "email", in the text colour.</summary>
        CategoryEmail,
        /// <summary>Integration category glyph "calendars", in the text colour.</summary>
        CategoryCalendars,
        /// <summary>Integration category glyph "productivity", in the text colour.</summary>
        CategoryProductivity,
        /// <summary>Integration category glyph "project-management", in the text colour.</summary>
        CategoryProjectManagement,
        /// <summary>Integration category glyph "development", in the text colour.</summary>
        CategoryDevelopment,
        /// <summary>Integration category glyph "storage", in the text colour.</summary>
        CategoryStorage,
        /// <summary>Integration category glyph "crm", in the text colour.</summary>
        CategoryCrm,
        /// <summary>Integration category glyph "web", in the text colour.</summary>
        CategoryWeb,
        /// <summary>The backlog: your request (the Signal) waits in line while every other item on the roadmap is put in front of it. Plays once and stands on its last frame.</summary>
        PlayBacklog,
        /// <summary>The spreadsheet: a header, and three rows keyed in by hand one cell at a time behind a cursor, cleared and typed again every week. Plays once and stands on its last frame.</summary>
        PlaySpreadsheet,
        /// <summary>The dead prototype: a small app builds itself on sample data, loses cells once it meets real systems and real permissions, and falls in a heap. Plays once and stands on its last frame.</summary>
        PlayDeadPrototype,
        /// <summary>Developer loop "Hybrid by default" (search).</summary>
        LoopSearchHybridByDefault,
        /// <summary>Developer loop "Permission-aware" (search).</summary>
        LoopSearchPermissionAware,
        /// <summary>Developer loop "Traversal in the query" (search).</summary>
        LoopSearchTraversalInTheQuery,
        /// <summary>Developer loop "Facets and filters" (search).</summary>
        LoopSearchFacetsAndFilters,
        /// <summary>Developer loop "Ranking you can inspect" (search).</summary>
        LoopSearchRankingYouCanInspect,
        /// <summary>Developer loop "Sub-second at scale" (search).</summary>
        LoopSearchSubSecondAtScale,
        /// <summary>Developer loop "Types you define" (knowledge-graph).</summary>
        LoopGraphTypesYouDefine,
        /// <summary>Developer loop "Traversal at speed" (knowledge-graph).</summary>
        LoopGraphTraversalAtSpeed,
        /// <summary>Developer loop "Revisions kept" (knowledge-graph).</summary>
        LoopGraphRevisionsKept,
        /// <summary>Developer loop "Records stay put" (knowledge-graph).</summary>
        LoopGraphRecordsStayPut,
        /// <summary>Developer loop "Permissions on edges" (knowledge-graph).</summary>
        LoopGraphPermissionsOnEdges,
        /// <summary>Developer loop "Incremental updates" (knowledge-graph).</summary>
        LoopGraphIncrementalUpdates,
        /// <summary>Developer loop "Entity extraction" (data-enrichment).</summary>
        LoopEnrichmentEntityExtraction,
        /// <summary>Developer loop "Classification" (data-enrichment).</summary>
        LoopEnrichmentClassification,
        /// <summary>Developer loop "Identifier normalization" (data-enrichment).</summary>
        LoopEnrichmentIdentifierNormalization,
        /// <summary>Developer loop "Documents and media" (data-enrichment).</summary>
        LoopEnrichmentDocumentsAndMedia,
        /// <summary>Developer loop "Your own pipelines" (data-enrichment).</summary>
        LoopEnrichmentYourOwnPipelines,
        /// <summary>Developer loop "Incremental" (data-enrichment).</summary>
        LoopEnrichmentIncremental,
        /// <summary>Developer loop "Model of your choice" (embeddings).</summary>
        LoopEmbeddingsModelOfYourChoice,
        /// <summary>Developer loop "No second datastore" (embeddings).</summary>
        LoopEmbeddingsNoSecondDatastore,
        /// <summary>Developer loop "Chunking that respects structure" (embeddings).</summary>
        LoopEmbeddingsChunkingThatRespectsStructure,
        /// <summary>Developer loop "Fused retrieval" (embeddings).</summary>
        LoopEmbeddingsFusedRetrieval,
        /// <summary>Developer loop "Permission-aware" (embeddings).</summary>
        LoopEmbeddingsPermissionAware,
        /// <summary>Developer loop "Re-embedding handled" (embeddings).</summary>
        LoopEmbeddingsReEmbeddingHandled,
        /// <summary>Developer loop "Tools over the graph" (ai-agents).</summary>
        LoopAgentsToolsOverTheGraph,
        /// <summary>Developer loop "Acting as a user" (ai-agents).</summary>
        LoopAgentsActingAsAUser,
        /// <summary>Developer loop "Full trace" (ai-agents).</summary>
        LoopAgentsFullTrace,
        /// <summary>Developer loop "Your own tools" (ai-agents).</summary>
        LoopAgentsYourOwnTools,
        /// <summary>Developer loop "Any model" (ai-agents).</summary>
        LoopAgentsAnyModel,
        /// <summary>Developer loop "Long-running work" (ai-agents).</summary>
        LoopAgentsLongRunningWork,
        /// <summary>Developer loop "70+ systems" (integrations).</summary>
        LoopIntegrationsSeventyPlusSystems,
        /// <summary>Developer loop "Structure preserved" (integrations).</summary>
        LoopIntegrationsStructurePreserved,
        /// <summary>Developer loop "Permissions synced" (integrations).</summary>
        LoopIntegrationsPermissionsSynced,
        /// <summary>Developer loop "Connector SDK" (integrations).</summary>
        LoopIntegrationsConnectorSdk,
        /// <summary>Developer loop "Incremental sync" (integrations).</summary>
        LoopIntegrationsIncrementalSync,
        /// <summary>Developer loop "Nothing moves" (integrations).</summary>
        LoopIntegrationsNothingMoves,
        /// <summary>Developer loop "C# SDKs" (developer-tools).</summary>
        LoopToolsCSharpSDKs,
        /// <summary>Developer loop "REST APIs" (developer-tools).</summary>
        LoopToolsRestAPIs,
        /// <summary>Developer loop "Build environment" (developer-tools).</summary>
        LoopToolsBuildEnvironment,
        /// <summary>Developer loop "Query inspection" (developer-tools).</summary>
        LoopToolsQueryInspection,
        /// <summary>Developer loop "Versioned configuration" (developer-tools).</summary>
        LoopToolsVersionedConfiguration,
        /// <summary>Developer loop "Operational visibility" (developer-tools).</summary>
        LoopToolsOperationalVisibility,
        /// <summary>Developer loop "Custom entity types" (extensibility).</summary>
        LoopExtensibilityCustomEntityTypes,
        /// <summary>Developer loop "Custom endpoints" (extensibility).</summary>
        LoopExtensibilityCustomEndpoints,
        /// <summary>Developer loop "Custom interfaces" (extensibility).</summary>
        LoopExtensibilityCustomInterfaces,
        /// <summary>Developer loop "Pipeline steps" (extensibility).</summary>
        LoopExtensibilityPipelineSteps,
        /// <summary>Developer loop "Agent tools" (extensibility).</summary>
        LoopExtensibilityAgentTools,
        /// <summary>Developer loop "Same permissions" (extensibility).</summary>
        LoopExtensibilitySamePermissions,
        /// <summary>Developer loop "Relationship-based permissions" (security).</summary>
        LoopSecurityRelationshipBasedPermissions,
        /// <summary>Developer loop "Synced from the source" (security).</summary>
        LoopSecuritySyncedFromTheSource,
        /// <summary>Developer loop "SSO with enforced MFA" (security).</summary>
        LoopSecuritySsoWithEnforcedMfa,
        /// <summary>Developer loop "Full audit trail" (security).</summary>
        LoopSecurityFullAuditTrail,
        /// <summary>Developer loop "Your infrastructure" (security).</summary>
        LoopSecurityYourInfrastructure,
        /// <summary>Developer loop "No training on your data" (security).</summary>
        LoopSecurityNoTrainingOnYourData,
    }

    internal static class PixelGlyphCatalog
    {
        /// <summary>Each glyph's animation in the pixel notation, how it plays, and whether empty cells show a hairline.</summary>
        public static void Get(PixelGlyphKind glyph, out string animation, out PixelGlyphPlayback playback, out bool showEmptyCells)
        {
            showEmptyCells = false;
            switch (glyph)
            {
                case PixelGlyphKind.ValueConnected: animation = PixelGlyph.BuildAnimation("...*..k..k..k..."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.ValueResolved: animation = PixelGlyph.BuildAnimation(".k..k*k..k......"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.ValueTraceable: animation = PixelGlyph.BuildAnimation(".*...k...k..kkk."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.ValueSovereign: animation = PixelGlyph.BuildAnimation("kkk.k*k.kkk....."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.ValueFast: animation = PixelGlyph.BuildAnimation("..k.kkk*..k....."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.ValueOpen: animation = PixelGlyph.BuildAnimation("k..*k...kkk....."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.ValuePrecise: animation = PixelGlyph.BuildAnimation("k.k..*..k.k....."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.ValueCurious: animation = PixelGlyph.BuildAnimation("...*kk..kkk.kkk."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.UseCaseKnowledgeManagement: animation = PixelGlyph.BuildAnimation("kkk.k*k.kkk....k"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.UseCaseTechnicalCustomerSupport: animation = PixelGlyph.BuildAnimation("kkkkk.*kkkkkk..."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.UseCaseEngineering: animation = PixelGlyph.BuildAnimation(".k*k.kk..k..k..."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.UseCaseQualityManagement: animation = PixelGlyph.BuildAnimation("...*..kkkkk..k.."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.UseCaseContractsAndCompliance: animation = PixelGlyph.BuildAnimation("kk*.k.kkk..kkkkk"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CapabilityGraph: animation = PixelGlyph.BuildAnimation("d..d.dd..dd.d..d"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CapabilityRetrieval: animation = PixelGlyph.BuildAnimation("dd.dddddd..d...d"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CapabilityPermissions: animation = PixelGlyph.BuildAnimation(".dd.d..ddddddd.d"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CapabilityModels: animation = PixelGlyph.BuildAnimation("dd..dd....dd..dd"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CapabilityConnectors: animation = PixelGlyph.BuildAnimation(".dd.dddddddd.dd."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CapabilityMemory: animation = PixelGlyph.BuildAnimation("dddddddddddddd.."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryCommunication: animation = PixelGlyph.BuildAnimation("kkkkk..kkkkkk..."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryEmail: animation = PixelGlyph.BuildAnimation("kkkk.kk.k..kkkkk"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryCalendars: animation = PixelGlyph.BuildAnimation("k..kkkkkkk.kkkkk"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryProductivity: animation = PixelGlyph.BuildAnimation("...k..kk.kkkkkkk"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryProjectManagement: animation = PixelGlyph.BuildAnimation("kk.kkk.kk..kk..."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryDevelopment: animation = PixelGlyph.BuildAnimation("kkk..kk..kkkkk.."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryStorage: animation = PixelGlyph.BuildAnimation("kk..kkkkk..kkkkk"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryCrm: animation = PixelGlyph.BuildAnimation(".kk..kk.kkkkk..k"); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.CategoryWeb: animation = PixelGlyph.BuildAnimation(".kk.k..kkkkk.kk."); playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.PlayBacklog: animation = "*.../..../..../....:600:Position_01 k*../..../..../....:170:Position_02 ks*./..../..../....:170:Position_03 ksk*/..../..../....:170:Position_04 kska/*.../..../....:170:Position_05 kska/k*../..../....:170:Position_06 kska/kk*./..../....:170:Position_07 kska/kka*/..../....:170:Position_08 kska/kkas/*.../....:170:Position_09 kska/kkas/k*../....:170:Position_10 kska/kkas/ks*./....:170:Position_11 kska/kkas/ksk*/....:170:Position_12 kska/kkas/kskk/*...:170:Position_13 kska/kkas/kskk/a*..:170:Position_14 kska/kkas/kskk/as*.:170:Position_15 kska/kkas/kskk/ask*:170:Position_16"; playback = PixelGlyphPlayback.Once; showEmptyCells = true; return;
                case PixelGlyphKind.PlaySpreadsheet: animation = "kkkk/..../..../....:260:Week_1 kkkk/o.../..../....:70 kkkk/so../..../....:70 kkkk/sao./..../....:70 kkkk/saao/..../....:70 kkkk/saaa/o.../....:70 kkkk/saaa/so../....:70 kkkk/saaa/sao./....:70 kkkk/saaa/saao/....:70 kkkk/saaa/saaa/o...:70 kkkk/saaa/saaa/so..:70 kkkk/saaa/saaa/sao.:70 kkkk/saaa/saaa/saao:70 kkkk/saaa/saaa/saaa:520 kkkk/..../..../....:260:Week_2 kkkk/o.../..../....:70 kkkk/so../..../....:70 kkkk/sao./..../....:70 kkkk/saao/..../....:70 kkkk/saaa/o.../....:70 kkkk/saaa/so../....:70 kkkk/saaa/sao./....:70 kkkk/saaa/saao/....:70 kkkk/saaa/saaa/o...:70 kkkk/saaa/saaa/so..:70 kkkk/saaa/saaa/sao.:70 kkkk/saaa/saaa/saao:70 kkkk/saaa/saaa/saaa:520 kkkk/..../..../....:260:Week_3 kkkk/o.../..../....:70 kkkk/so../..../....:70 kkkk/sao./..../....:70 kkkk/saao/..../....:70 kkkk/saaa/o.../....:70 kkkk/saaa/so../....:70 kkkk/saaa/sao./....:70 kkkk/saaa/saao/....:70 kkkk/saaa/saaa/o...:70 kkkk/saaa/saaa/so..:70 kkkk/saaa/saaa/sao.:70 kkkk/saaa/saaa/saao:70 kkkk/saaa/saaa/saaa:520"; playback = PixelGlyphPlayback.Once; showEmptyCells = true; return;
                case PixelGlyphKind.PlayDeadPrototype: animation = "k.../..../..../....:45:Demo kk../..../..../....:45 kkk./..../..../....:45 kkkk/..../..../....:45 kkkk/k.../..../....:45 kkkk/ks../..../....:45 kkkk/ksa./..../....:45 kkkk/ksak/..../....:45 kkkk/ksak/k.../....:45 kkkk/ksak/ka../....:45 kkkk/ksak/kaa./....:45 kkkk/ksak/kaak/....:45 kkkk/ksak/kaak/k...:45 kkkk/ksak/kaak/kk..:45 kkkk/ksak/kaak/kkk.:45 kkkk/ksak/kaak/kkkk:700 kkkk/k.ak/kaak/kkkk:150:Real kkkk/k.ak/kaa./kkkk:150 kk.k/k.ak/kaa./kkkk:150 kk.k/k.ak/kaa./.kkk:150 kk.k/k.a./kaa./.kkk:150 k..k/k.a./kaa./.kkk:150 k..k/k.a./kaa./.k.k:150 a..a/a.a./aaa./.a.a:380 ..../a..a/aaa./aaaa:110:Dead ..../a.../aaaa/aaaa:110"; playback = PixelGlyphPlayback.Once; showEmptyCells = true; return;
                case PixelGlyphKind.LoopSearchHybridByDefault: animation = "k.a./..../..../.... k.a./k.a./..../.... k.a./k.a./.*../.... k.a./k.a./.*../.k.."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSearchPermissionAware: animation = "kaka/..../ssss/.... ..../kaka/ssss/.... ..../..../ksks/.... ..../..../ssss/k.*."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSearchTraversalInTheQuery: animation = "k.../..../..../.... k.../k.../..../.... k.../kk../..../.... k.../kk../.k../.... k.../kk../.kk./.... k.../kk../.kk./..k. k.../kk../.kk./..k*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSearchFacetsAndFilters: animation = "kaaa/..../aaak/.... akaa/..../aaka/.... aaka/..../akaa/.... aaka/..../a*aa/...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSearchRankingYouCanInspect: animation = "kk../kkkk/k.../kkk. kkkk/kk../kkk./k... kkkk/kkk./kk../k... kkk*/kkk./kk../k..."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSearchSubSecondAtScale: animation = "..../k.../..../....:70 ..../ak../..../....:70 ..../aak./..../....:70 ..../aaak/..../....:70 ..k./kkk*/..k./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopGraphTypesYouDefine: animation = "oo../oo../..../.... kk../kk../..../.... kk../kk../..oo/..oo kk../kk../..aa/..a*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopGraphTraversalAtSpeed: animation = "*..a/.a../..a./a..k:90 k..a/.*../..a./a..k:90 k..a/.a../..*./a..k:90 k..a/.a../..a./a..*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopGraphRevisionsKept: animation = "ss../ss../..../.... ss../saa./.aa./.... ss../saa./.akk/..kk ss../s*a./.akk/..kk"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopGraphRecordsStayPut: animation = ".k../..../..../....:120 kkk./.k../..../....:120 k*k./kkk./.k../....:120 kkk./k*k./kkk./.k..:260 k*k./kkk./.k../....:120 kkk./k*k./kkk./.k.."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopGraphPermissionsOnEdges: animation = "k.../..../..../.... ka../s.../..../.... kaa./s.../s.../.... kaak/s.../s.../s... kaak/s..a/s.../s... kaak/s..a/s..a/s... kaak/s..a/s..a/s..*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopGraphIncrementalUpdates: animation = "k.*./.a../k.a./....:260 k.k./.*../k.a./....:260 k.k./.a../*.a./....:260 k.k./.a../k.*./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEnrichmentEntityExtraction: animation = "..../aaaa/aaaa/aa.. ..../aaka/aaaa/aa..:260 ..k./aa.a/aaaa/aa.. ..*./aa.a/aaaa/aa.."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEnrichmentClassification: animation = "..../..../..../k..a .k../..../..../k..a ..../k.../..../k..a ..../..../k.../k..a ..a./..../k.../k..a ..../...a/k.../k..a ..../..../k..a/k..a ..*./..../k..a/k..a"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEnrichmentIdentifierNormalization: animation = "k.kk/.kk./kk.k/....:360 kkk./kkk./kkk./....:300 aaa./kkk*/aaa./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEnrichmentDocumentsAndMedia: animation = "kkk./k.k./k.k./kkk. kkk./kak./k.k./kkk. kkk./kaka/k.k./kkk. kkk./kaka/kak./kkk. kkk./kaka/kak*/kkk."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEnrichmentYourOwnPipelines: animation = "..*./aa.a/..../.... ..../aa*a/..../.... ..../aa*a/..../a... ..../aa*a/..../.a.. ..../aa*a/..../..k. ..../aa*a/..../...k"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEnrichmentIncremental: animation = "kkk./..../..../.... kkk./aaa./..../.... kkk./a*a./..../.... kkk./kkk./..../.... kkk./kkk./aaa./.... kkk./kkk./a*a./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEmbeddingsModelOfYourChoice: animation = ".kk./a..a/a..a/aaaa ..../akka/akka/aaaa:320 .oo./a..a/a..a/aaaa ..../aooa/aooa/aaaa:320 .kk./a..a/a..a/aaaa ..../akka/akka/aa*a"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEmbeddingsNoSecondDatastore: animation = "kkk./k.k./kkk./...a kkk./k.k./kkka/.... kkk./k.ka/kkk./.... kkk./k*k./kkk./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEmbeddingsChunkingThatRespectsStructure: animation = "k.../..../..../.... k.../aaa./..../.... k.../aaa./k.../.... k.../aaa./k.../aa*."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEmbeddingsFusedRetrieval: animation = "kkk./aaa./ooo./.... kk../aa../oo../k... kk../aa../oo../ka.. kk../aa../oo../kao. kk../aa../oo../kao*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEmbeddingsPermissionAware: animation = "..../.*../..../.... .k../k*k./.k../....:320 .s../k*s./.k../...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopEmbeddingsReEmbeddingHandled: animation = "a.a./.a.a/a.a./.a.a k.a./.a.a/k.a./.a.a k.a./.k.a/k.a./.k.a k.k./.k.a/k.k./.k.a k.k./.k.k/k.k./.k.k k.k./.k.k/k.k./.k.*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopAgentsToolsOverTheGraph: animation = "..../.*../..../.... ..../.*ak/..../.... ..../.*ak/.a../.k.. ..../.*ak/.aa./.k.k"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopAgentsActingAsAUser: animation = "kkkk/k*.k/k..k/kkkk:260 kkkk/k.*k/k..k/kkkk:260 kkkk/k..k/k.*k/kkkk:260 kkkk/k..k/k*.k/kkkk:260 kkkk/k*.k/k..k/kkkk"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopAgentsFullTrace: animation = "k.../..../..../.... ka../..../..../.... kaa./..../..../.... kaa./..a./..../.... kaa./..a./.a../.... kaa./..a./.a../*..."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopAgentsYourOwnTools: animation = "k.k./..../k.../.... k.k*/..../k.../.... k.k./...*/k.../.... k.k./..../k.*./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopAgentsAnyModel: animation = "...*/k..a/...a/...o ...a/k..*/...a/...o ...a/k..a/...*/...o ...a/kaa*/...a/...o"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopAgentsLongRunningWork: animation = "aa../k.../..../....:280 aa../kk../..../....:280 aa../kkk./..../....:280 aa../kkkk/..../....:280 aa../kkkk/k.../....:280 aa../kkkk/kk../....:280 aa../kkkk/kk*./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopIntegrationsSeventyPlusSystems: animation = "..../.k../..../....:60 ..../.k../..../..a.:60 k.../.k../..../..a.:60 k.../.k../..k./..a.:60 k..a/.k../..k./..a.:60 k..a/.k../k.k./..a.:60 k..a/.k../k.k./.ka.:60 ka.a/.k../k.k./.ka.:60 ka.a/.ka./k.k./.ka.:60 ka.a/.ka./k.ka/.ka.:60 ka.a/aka./k.ka/.ka.:60 ka.a/aka./kaka/.ka.:60 kaka/aka./kaka/.ka.:60 kaka/aka./kaka/aka.:60 kaka/akak/kaka/aka.:60 kaka/akak/kaka/aka*:60"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopIntegrationsStructurePreserved: animation = "k.../a.../..../.... kk../aa../..../.... kk../aa../...k/...a kk../aa../..kk/..a*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopIntegrationsPermissionsSynced: animation = "k.../a.../k.../k... k..k/a.../k.../k... k..k/a..a/k.../k... k..k/a..a/k..k/k... k..k/a..a/k..k/k..*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopIntegrationsConnectorSdk: animation = "...a/k..a/...a/...a ...a/kk.a/k..a/...a ...a/kkka/kk.a/...a ...a/kkk*/kk.a/...a"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopIntegrationsIncrementalSync: animation = "..../kk.a/k.ka/.... ..../k.ka/kk.a/.... ..../kk.a/k.ka/.... ..../k.ka/kk.a/.... ..../k.k*/kk.a/...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopIntegrationsNothingMoves: animation = "..../..../..kk/..kk a.../..../..kk/..kk a.../.a../..kk/..kk a.../.a../..kk/..k*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopToolsCSharpSDKs: animation = "*.../..../..../.... k*../..../..../.... kk*./..../..../.... kka*/..../..../.... kka./.*../..../.... kka./.k*./..../.... kka./.ka*/..../.... kka./.kaa/*.../.... kka./.kaa/k*../....:400 kka./.kaa/k.../....:400 kka./.kaa/k*../...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopToolsRestAPIs: animation = "..../k..k/k..k/.... ..../k*.k/k..k/.... ..../k.*k/k..k/.... ..../k..k/k.ak/.... ..../k..k/ka.k/.... ..../k*.k/k.ak/...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopToolsBuildEnvironment: animation = "kkkk/k..k/kkkk/.kk. kkkk/ka.k/kkkk/.kk. kkkk/kaak/kkkk/.kk. kkkk/ka*k/kkkk/.kk."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopToolsQueryInspection: animation = "*kkk/.kkk/..kk/...k kkkk/.*kk/..kk/...k kkkk/.kkk/..*k/...k kkkk/.kkk/..kk/...*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopToolsVersionedConfiguration: animation = "k.../..../..../.... ka../..../..../.... ka../..a./..../.... ka../..a./..a./.... ka../k.a./..a./.... ka../k.a./k.a./.... ka../k.a./k.a./k... ka../k.a./k.a./k*.."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopToolsOperationalVisibility: animation = "..../..../.k.k/k.k. ..../..../k.k./.k.k ...*/..../.k../k.k."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopExtensibilityCustomEntityTypes: animation = "k.../..../..../.... ka../..../..../.... kaa./..../..../.... kaa./a.../..../.... kaa./a.../a.../.... kaa./a.../a.../k... kaa*/a.../a.../k..."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopExtensibilityCustomEndpoints: animation = "kkk./k.k./kkk./.... kkk./k.k*/kkk./.... kkk./k*k./kkk./....:300 kkk./k.k*/kkk./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopExtensibilityCustomInterfaces: animation = "kkkk/k.../k.../k... kkkk/k.aa/k.../k... kkkk/k.aa/k.aa/k... kkkk/k.aa/k.aa/k.a*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopExtensibilityPipelineSteps: animation = "aaa./aaa./..../aaa. aaa./aaa./...*/aaa. aaa./aaa./..*k/aaa. aaa./aaa./.*kk/aaa. aaa./aaa./*kk./aaa."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopExtensibilityAgentTools: animation = "kk.a/kk../..../.... kk.a/kk.a/..../.... kk.a/kk.a/...a/.... kk.a/kk.a/...a/...*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopExtensibilitySamePermissions: animation = "k.kk/..../..../.... k.kk/o.oo/..../.... k.kk/k.kk/..../.... k.kk/k.kk/o.oo/.... k.kk/k.kk/k.k*/...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSecurityRelationshipBasedPermissions: animation = "aaa./aaa./aaa./.... kaa./aaa./aaa./.... kaa./aak./aaa./.... kaa./aak./a*a./...."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSecuritySyncedFromTheSource: animation = "kkk./kkk./..../.... kkk./kkk./.a../.... kkk./kkk./.a../.*.."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSecuritySsoWithEnforcedMfa: animation = "kkkk/kk.k/..../....:320 kkkk/kk.k/..../a... kkkk/kk.k/..../aa.. kkkk/kk.k/..../aaa. kkkk/kk.k/..../aaa*"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSecurityFullAuditTrail: animation = "..../..../..../kka. ..../..../kka./ka.. ..../kka./ka../kkk. kka./ka../kkk./ka*."; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSecurityYourInfrastructure: animation = "kkk*/kkka/kkka/a..a:260 kkka/kkk*/kkka/a..a:260 kkka/kkka/kkk*/a..a"; playback = PixelGlyphPlayback.Loop; return;
                case PixelGlyphKind.LoopSecurityNoTrainingOnYourData: animation = "kkk./k*k./kkk./...a:420 kkk./k*k./kkka/....:520 kkk./k*k./kkk./...a"; playback = PixelGlyphPlayback.Loop; return;
                default: animation = "................"; playback = PixelGlyphPlayback.Once; return;
            }
        }
    }
}
