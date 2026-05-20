/**
 * @compiler H5 26.5.1027+64d8425b5ec4c14f79bd5f1aea78dd21dc4729cc
 */
H5.assemblyVersion("Tesserae.Tests","2026.5.66668.0");
H5.assembly("Tesserae.Tests", function ($asm, globals) {
    "use strict";

    H5.define("Tesserae.Tests.App", {
        main: function Main () {
            var $t, $t1, $t2, $t3;
            var SelectSidebar = null;
            document.body.style.overflow = "hidden";

            if ((document.head.querySelector("meta[name='viewport']") == null)) {
                var viewportMeta = document.createElement("meta");
                viewportMeta.name = "viewport";
                viewportMeta.content = "width=device-width, initial-scale=1.0, maximum-scale=5.0";
                document.head.appendChild(viewportMeta);
            }

            tss.UI.Theme.EnableMobileDetection(768);

            var allSidebarItems = new (System.Collections.Generic.List$1(Tesserae.ISidebarItem)).ctor();
            var sampleToSidebarItem = new (System.Collections.Generic.Dictionary$2(Tesserae.Tests.Sample,Tesserae.ISidebarItem)).ctor();



            var currentPage = new (tss.SettableObservableT(Tesserae.Tests.Sample))(null);
            SelectSidebar = function (toSelect) {
                allSidebarItems.ForEach(function (i) {
                    i.Tesserae$ISidebarItem$IsSelected = H5.rE(i, toSelect);
                });
            };

            currentPage.Observe(function (selected) {
                var item = { };
                if (H5.is(selected, System.Object) && sampleToSidebarItem.tryGetValue(selected, item)) {
                    SelectSidebar(item.v);
                }
            });

            var sidebar = tss.UI.Sidebar(true);

            if (tss.UI.Theme.IsMobileMode) {
                sidebar.AsNavbar();
            }

            var sortingTimeout = 0.0;

            sidebar.OnSortingChanged(function (itemOrder) {
                window.clearTimeout(sortingTimeout);

                sortingTimeout = window.setTimeout(function (_) {
                    var $t;
                    var itemOrderObject = { };

                    $t = H5.getEnumerator(itemOrder);
                    try {
                        while ($t.moveNext()) {
                            var item = $t.Current;
                            itemOrderObject[item.key] = item.value;
                        }
                    } finally {
                        if (H5.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                    var sorting = { itemOrder: itemOrderObject };

                    localStorage.setItem(Tesserae.Tests.App._sidebarOrderKey, JSON.stringify(sorting));
                    console.log("saved sorting", sorting);
                }, 1000);
            });


            sidebar.AddHeader(new Tesserae.SidebarText("header", "Tesserae", "TSS", "tss-fontsize-xlarge", "tss-fontweight-bold"));

            var searchBox = new Tesserae.SidebarSearchBox("search", "Search...");
            searchBox.OnSearch(function (term) {
                sidebar.Search(term);
            });
            sidebar.AddHeader(searchBox);

            var contentArea = tss.UI.DeferSync$3(Tesserae.Tests.Sample, currentPage, function (page) {
                return (page == null) ? H5.cast(tss.UI.CenteredCardWithBackground(tss.UI.Message("Welcome to Tesserae", "Select a component to see more details").Icon$1(3936)), tss.IC) : tss.ICTX.Children$6(tss.S, tss.ScrollBar.ScrollY(tss.S, tss.ICX.S(tss.S, tss.UI.VStack())), [tss.ICX.MinHeight(tss.IC, tss.ICX.WS(tss.IC, page.ContentGenerator()), tss.usX.percent$1(100))]);
            });

            var pageContent;

            if (tss.UI.Theme.IsMobileMode) {
                pageContent = tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.Class(tss.S, tss.UI.VStack(), "tss-page-layout")), [tss.ICX.WS(tss.Sidebar, sidebar), tss.ICX.Grow(tss.IDefer, tss.ICX.H(tss.IDefer, tss.ICX.WS(tss.IDefer, contentArea), 1))]);
            } else {
                pageContent = tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.Class(tss.S, tss.UI.HStack(), "tss-page-layout")), [tss.ICX.HS(tss.Sidebar, sidebar), tss.ICX.Grow(tss.IDefer, tss.ICX.W(tss.IDefer, tss.ICX.HS(tss.IDefer, contentArea), 1))]);
            }

            tss.UI.MountToBody(pageContent);


            var samples = System.Linq.Enumerable.from(H5.Reflection.getAssemblyTypes(H5.Reflection.getTypeAssembly(Tesserae.Tests.ISample)), System.Type).where(function (t) {
                    return H5.Reflection.isAssignableFrom(Tesserae.Tests.ISample, t) && !H5.Reflection.isInterface(t);
                }).select(function (sampleType) {
                var sg = H5.as(System.Linq.Enumerable.from(H5.Reflection.getAttributes(sampleType, Tesserae.Tests.SampleDetailsAttribute, true), System.Object).firstOrDefault(null, null), Tesserae.Tests.SampleDetailsAttribute);
                var group = H5.is(sg, System.Object) ? sg.Group : "Others";
                var order = H5.is(sg, System.Object) ? sg.Order : 0;
                var icon = H5.is(sg, System.Object) ? sg.Icon : 1014;
                return new Tesserae.Tests.Sample(H5.Reflection.getTypeName(sampleType), Tesserae.Tests.Sample.FormatSampleName$1(sampleType), group, order, icon, function () {
                    return H5.as(H5.createInstance(sampleType), tss.IC);
                });
            }).toDictionary(function (s) {
                    return s.Name;
                }, function (s) {
                    return s;
                }, System.String, Tesserae.Tests.Sample);

            sidebar.AddHeader(new Tesserae.SidebarButton.$ctor10("SOURCE_CODE", 696, "Source Code", [new Tesserae.SidebarCommand.$ctor7(237).Tooltip$1("Open repository on GitHub").OnClick(function () {
                window.open("https://github.com/curiosity-ai/tesserae", "_blank");
            })]).CommandsAlwaysVisible().OnOpenIconClick(function () {
                tss.UI.Toast().Success("You clicked on the icon");
            }));

            var openClose = new Tesserae.SidebarCommand.$ctor7(119).Tooltip$1("Close Sidebar");

            if (!tss.UI.Theme.IsMobileMode) {
                var v = { };
                var sidebarOpenState = System.Boolean.tryParse(localStorage.getItem(Tesserae.Tests.App._sidebarOpenStateKey), v) ? v.v : true;
                sidebar.Closed(!sidebarOpenState);
            }

            openClose.OnClick(function () {
                sidebar.Toggle();

                if (sidebar.IsClosed) {
                    openClose.SetIcon$1(120).Tooltip$1("Open Sidebar");
                    localStorage.setItem(Tesserae.Tests.App._sidebarOpenStateKey, System.Boolean.toString((false)));
                } else {
                    openClose.SetIcon$1(119).Tooltip$1("Close Sidebar");
                    localStorage.setItem(Tesserae.Tests.App._sidebarOpenStateKey, System.Boolean.toString((true)));
                }
            });

            var lightDark = new Tesserae.SidebarCommand.$ctor7(4391).Tooltip$1("Light Mode");

            lightDark.OnClick(function () {
                if (tss.UI.Theme.IsDark) {
                    tss.UI.Theme.Light();
                    lightDark.SetIcon$1(4391).Tooltip$1("Light Mode");
                } else {
                    tss.UI.Theme.Dark();
                    lightDark.SetIcon$1(3053).Tooltip$1("Dark Mode");
                }
            });

            var commandSidebarconfig = new Tesserae.SidebarCommands("CONFIG", [lightDark, openClose]);
            sidebar.AddFooter(commandSidebarconfig);

            var groupIndex = 0;

            $t = H5.getEnumerator(System.Linq.Enumerable.from(samples.Values, Tesserae.Tests.Sample).groupBy(function (s) {
                    return s.Group;
                }).orderBy(function (g) {
                return g.key();
            }));
            try {
                while ($t.moveNext()) {
                    var group = $t.Current;
                    var groupKey = (group.key() || "") + H5.identity(groupIndex, ((groupIndex = (groupIndex + 1) | 0)));

                    var separator = new Tesserae.SidebarSeparator(groupKey, group.key());
                    sidebar.AddContent(separator);

                    var itemIndex = 0;

                    $t1 = H5.getEnumerator(group.orderBy(function (s) {
                        return s.Order;
                    }).thenBy(function (s) {
                        return s.Name.toLowerCase();
                    }));
                    try {
                        while ($t1.moveNext()) {
                            var item = { v : $t1.Current };
                            var sidebarItem = new Tesserae.SidebarButton.$ctor14((item.v.Name || "") + H5.identity(itemIndex, ((itemIndex = (itemIndex + 1) | 0))), item.v.Icon, item.v.Name, [new Tesserae.SidebarCommand.$ctor7(4301).Tooltip$1("Show sample code").OnClick((function ($me, item) {
                                return function () {
                                    Tesserae.Tests.Samples.SamplesHelper.ShowSampleCode(item.v.Type);
                                };
                            })(this, item)), new Tesserae.SidebarCommand.$ctor7(237).Tooltip$1("Open in new tab").OnClick((function ($me, item) {
                                return function () {
                                    window.open(System.String.format("#/view/{0}", [item.v.Name]), "_blank");
                                };
                            })(this, item))]);

                            sidebarItem.OnClick((function ($me, item) {
                                return function () {
                                    tss.Router.Push(System.String.format("#/view/{0}", [item.v.Name]));
                                    currentPage.Value$1 = item.v;
                                };
                            })(this, item));


                            sidebar.AddContent(sidebarItem);
                            allSidebarItems.add(sidebarItem);
                            sampleToSidebarItem.setItem(item.v, sidebarItem);
                        }
                    } finally {
                        if (H5.is($t1, System.IDisposable)) {
                            $t1.System$IDisposable$Dispose();
                        }
                    }
                }
            } finally {
                if (H5.is($t, System.IDisposable)) {
                    $t.System$IDisposable$Dispose();
                }
            }


            var sidebarOrderJson = localStorage.getItem(Tesserae.Tests.App._sidebarOrderKey);

            if (H5.is(sidebarOrderJson, System.Object)) {
                var sidebarOrderObj = JSON.parse(sidebarOrderJson);
                console.log("loaded sorting", H5.unbox(sidebarOrderObj));


                var itemOrderObj = H5.unbox(sidebarOrderObj.itemOrder);
                if ((itemOrderObj == null)) {
                    return;
                }
                var itemOrder = new (System.Collections.Generic.Dictionary$2(System.String,System.Array.type(System.String))).ctor();

                $t2 = H5.getEnumerator(Object.getOwnPropertyNames(H5.unbox(itemOrderObj)));
                try {
                    while ($t2.moveNext()) {
                        var identifier = $t2.Current;
                        itemOrder.setItem(identifier, H5.unbox(itemOrderObj[identifier]));

                    }
                } finally {
                    if (H5.is($t2, System.IDisposable)) {
                        $t2.System$IDisposable$Dispose();
                    }
                }
                sidebar.LoadSorting(itemOrder);
            }

            tss.Router.Register$3("home", "/", function (_) {
                currentPage.Value$1 = null;
            });


            var documentTitleBase = document.title;

            $t3 = H5.getEnumerator(samples);
            try {
                while ($t3.moveNext()) {
                    var kv = { v : $t3.Current };
                    tss.Router.Register(System.String.format("#/view/{0}", [System.String.replaceAll(kv.v.key, " ", "%20")]), (function ($me, kv) {
                        return function (_) {
                            currentPage.Value$1 = kv.v.value;
                        };
                    })(this, kv));
                }
            } finally {
                if (H5.is($t3, System.IDisposable)) {
                    $t3.System$IDisposable$Dispose();
                }
            }

            tss.Router.Initialize();
            tss.Router.Refresh(tss.Router.ForceMatchCurrent);
        },
        statics: {
            fields: {
                _sidebarOrderKey: null,
                _sidebarOpenStateKey: null
            },
            ctors: {
                init: function () {
                    this._sidebarOrderKey = "tss-sample-sidebar-order";
                    this._sidebarOpenStateKey = "tss-sample-sidebar-open-close";
                }
            }
        }
    });

    H5.define("Tesserae.Tests.ISample", {
        $kind: "interface"
    });

    H5.define("Tesserae.Tests.Sample", {
        statics: {
            methods: {
                FormatSampleName$1: function (sampleType) {
                    return Tesserae.Tests.Sample.FormatSampleName(H5.Reflection.getTypeName(sampleType));
                },
                FormatSampleName: function (sampleType) {
                    return System.String.replaceAll(System.String.replaceAll(H5.toArray(System.Linq.Enumerable.from(System.String.replaceAll(sampleType, "Sample", ""), System.Char).select(function (c) {
                                return H5.isUpper(c) ? " " + String.fromCharCode(c) : "" + String.fromCharCode(c);
                            })).join("").trim(), "U Icons", "UIcons"), " And ", " and ");
                }
            }
        },
        fields: {
            Type: null,
            Name: null,
            Group: null,
            Order: 0,
            Icon: 0,
            ContentGenerator: null
        },
        ctors: {
            ctor: function (type, name, group, order, icon, contentGenerator) {
                this.$initialize();
                this.Type = type;
                this.Name = name;
                this.Group = group;
                this.Order = order;
                this.Icon = icon;
                this.ContentGenerator = contentGenerator;
            }
        }
    });

    H5.define("Tesserae.Tests.SampleDetailsAttribute", {
        inherits: [System.Attribute],
        fields: {
            Group: null,
            Order: 0,
            Icon: 0
        }
    });

    H5.define("Tesserae.Tests.Samples.DetailsListSampleFileItem", {
        inherits: function () { return [tss.IDetailsListItemT(Tesserae.Tests.Samples.DetailsListSampleFileItem)]; },
        fields: {
            FileIcon: 0,
            FileName: null,
            DateModified: null,
            ModifiedBy: null,
            FileSize: 0
        },
        props: {
            EnableOnListItemClickEvent: {
                get: function () {
                    return true;
                }
            }
        },
        alias: [
            "EnableOnListItemClickEvent", "tss$IDetailsListItem$EnableOnListItemClickEvent",
            "OnListItemClick", "tss$IDetailsListItem$OnListItemClick",
            "CompareTo", ["tss$IDetailsListItemT$Tesserae$Tests$Samples$DetailsListSampleFileItem$CompareTo", "tss$IDetailsListItemT$CompareTo"],
            "Render", "tss$IDetailsListItem$Render"
        ],
        ctors: {
            init: function () {
                this.DateModified = System.DateTime.getDefaultValue();
            },
            ctor: function (icon, name, modified, by, size) {
                this.$initialize();
                this.FileIcon = icon;
                this.FileName = name;
                this.DateModified = modified;
                this.ModifiedBy = by;
                this.FileSize = size;
            }
        },
        methods: {
            OnListItemClick: function (index) {
                tss.UI.Toast().Information(System.String.format("Clicked: {0}", [this.FileName]));
            },
            CompareTo: function (other, key) {
                switch (key) {
                    case "FileName": 
                        return System.String.compare(this.FileName, other.FileName, 5);
                    case "DateModified": 
                        return H5.compare(this.DateModified, other.DateModified);
                    case "ModifiedBy": 
                        return System.String.compare(this.ModifiedBy, other.ModifiedBy, 5);
                    case "FileSize": 
                        return H5.compare(this.FileSize, other.FileSize);
                    default: 
                        return 0;
                }
            },
            Render: function (columns, cell) {
                return new (H5.GeneratorEnumerable$1(tss.IC))(H5.fn.bind(this, function (columns, cell) {
                    var $s = 0,
                        $jff,
                        $rv,
                        $ae;

                    var $en = new (H5.GeneratorEnumerator$1(tss.IC))(H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                switch ($s) {
                                    case 0: {
                                        $en.current = cell(System.Array.getItem(columns, 0, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.Icon$2(this.FileIcon);
                                            }));
                                            $s = 1;
                                            return true;
                                    }
                                    case 1: {
                                        $en.current = cell(System.Array.getItem(columns, 1, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(this.FileName);
                                            }));
                                            $s = 2;
                                            return true;
                                    }
                                    case 2: {
                                        $en.current = cell(System.Array.getItem(columns, 2, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(System.DateTime.format(this.DateModified, "d"));
                                            }));
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $en.current = cell(System.Array.getItem(columns, 3, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(this.ModifiedBy);
                                            }));
                                            $s = 4;
                                            return true;
                                    }
                                    case 4: {
                                        $en.current = cell(System.Array.getItem(columns, 4, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock((System.Double.format(this.FileSize, "N1") || "") + " KB");
                                            }));
                                            $s = 5;
                                            return true;
                                    }
                                    case 5: {

                                    }
                                    default: {
                                        return false;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            throw $ae;
                        }
                    }));
                    return $en;
                }, arguments));
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DetailsListSampleItemWithComponents", {
        inherits: function () { return [tss.IDetailsListItemT(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents)]; },
        fields: {
            Icon: 0,
            CheckBox: null,
            Name: null,
            Button: null,
            ChoiceGroup: null
        },
        props: {
            EnableOnListItemClickEvent: {
                get: function () {
                    return false;
                }
            }
        },
        alias: [
            "EnableOnListItemClickEvent", "tss$IDetailsListItem$EnableOnListItemClickEvent",
            "OnListItemClick", "tss$IDetailsListItem$OnListItemClick",
            "CompareTo", ["tss$IDetailsListItemT$Tesserae$Tests$Samples$DetailsListSampleItemWithComponents$CompareTo", "tss$IDetailsListItemT$CompareTo"],
            "Render", "tss$IDetailsListItem$Render"
        ],
        methods: {
            OnListItemClick: function (index) { },
            CompareTo: function (other, key) {
                return 0;
            },
            WithIcon: function (icon) {
                this.Icon = icon;
                return this;
            },
            WithCheckBox: function (cb) {
                this.CheckBox = cb;
                return this;
            },
            WithName: function (name) {
                this.Name = name;
                return this;
            },
            WithButton: function (btn) {
                this.Button = btn;
                return this;
            },
            WithChoiceGroup: function (cg) {
                this.ChoiceGroup = cg;
                return this;
            },
            Render: function (columns, cell) {
                return new (H5.GeneratorEnumerable$1(tss.IC))(H5.fn.bind(this, function (columns, cell) {
                    var $s = 0,
                        $jff,
                        $rv,
                        $ae;

                    var $en = new (H5.GeneratorEnumerator$1(tss.IC))(H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                switch ($s) {
                                    case 0: {
                                        $en.current = cell(System.Array.getItem(columns, 0, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.Icon$2(this.Icon);
                                            }));
                                            $s = 1;
                                            return true;
                                    }
                                    case 1: {
                                        $en.current = cell(System.Array.getItem(columns, 1, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.CheckBox;
                                            }));
                                            $s = 2;
                                            return true;
                                    }
                                    case 2: {
                                        $en.current = cell(System.Array.getItem(columns, 2, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(this.Name);
                                            }));
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $en.current = cell(System.Array.getItem(columns, 3, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.Button;
                                            }));
                                            $s = 4;
                                            return true;
                                    }
                                    case 4: {
                                        $en.current = cell(System.Array.getItem(columns, 4, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.ChoiceGroup;
                                            }));
                                            $s = 5;
                                            return true;
                                    }
                                    case 5: {

                                    }
                                    default: {
                                        return false;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            throw $ae;
                        }
                    }));
                    return $en;
                }, arguments));
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.EmojiSample.IconItem", {
        inherits: [tss.ISearchableItem],
        $kind: "nested class",
        fields: {
            _value: null,
            component: null
        },
        alias: [
            "IsMatch", "tss$ISearchableItem$IsMatch",
            "Render", "tss$ISearchableItem$Render"
        ],
        ctors: {
            ctor: function (icon, name) {
                this.$initialize();
                name = Tesserae.Tests.Samples.EmojiSample.ToValidName(name.substr(3));
                this._value = (name || "") + " " + (System.Enum.toString(Tesserae.Emoji, icon) || "");

                this.component = tss.ICTX.Children$6(tss.S, tss.ICX.PB(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()).AlignItemsCenter(), 4), [tss.ICX.MinWidth(tss.Icon, tss.UI.Icon(icon, "tss-fontsize-large"), tss.usX.px$1(36)), tss.ICX.Grow(tss.txt, tss.ICX.W(tss.txt, tss.txtX.Title(tss.txt, tss.txtX.Ellipsis(tss.txt, tss.UI.TextBlock(System.String.format("{0}", [name]))), System.Enum.toString(Tesserae.Emoji, icon)), 1))]);
            }
        },
        methods: {
            IsMatch: function (searchTerm) {
                return System.String.contains(this._value,searchTerm);
            },
            Render: function () {
                return this.component;
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.HashingHelper", {
        statics: {
            methods: {
                Fnv1aHash: function (value) {
                    var valArray = System.String.toCharArray(value, 0, value.length);
                    var hash = 2166136261;
                    for (var i = 0; i < valArray.length; i = (i + 1) | 0) {
                        hash = (hash ^ valArray[System.Array.index(i, valArray)]) >>> 0;
                        hash = H5.Int.umul(hash, 16777619);
                    }
                    return (hash | 0);
                }
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.NodeViewSample.ComplexNode", {
        inherits: [Tesserae.INodeView],
        $kind: "nested class",
        props: {
            Name: {
                get: function () {
                    return "Complex";
                }
            },
            Inputs: {
                get: function () {
                    return System.Array.init([new (Tesserae.NodeInput$1(System.String))("text", "Input", "Hi Input"), new (Tesserae.NodeInput$1(System.Int32))("int", "Input", 123), new (Tesserae.NodeInput$1(System.Double))("num", "Input", 3.14), new (Tesserae.NodeInput$1(Function))("btn", "Input", function () {
                        tss.UI.Toast().Information("Hi!");
                    }), new (Tesserae.NodeInput$1(System.Boolean))("chk", "Input", false), new Tesserae.NodeSelectInput("sel", "Input", "A", System.Array.init(["A", "B", "C"], System.String)), new Tesserae.NodeSliderInput("sld", "Input", 0.5, 0, 1)], Tesserae.INodeInput);
                }
            },
            Outputs: {
                get: function () {
                    return System.Array.init([new (Tesserae.NodeOutput$1(System.String))("out", "Output", "Hi Output")], Tesserae.INodeOutput);
                }
            }
        },
        alias: [
            "Name", "Tesserae$INodeView$Name",
            "Inputs", "Tesserae$INodeView$Inputs",
            "Outputs", "Tesserae$INodeView$Outputs"
        ]
    });

    H5.define("Tesserae.Tests.Samples.NodeViewSample.DynamicNode", {
        inherits: [Tesserae.IDynamicNodeView],
        $kind: "nested class",
        props: {
            Name: {
                get: function () {
                    return "Dynamic";
                }
            },
            Inputs: {
                get: function () {
                    return System.Array.init([new (Tesserae.NodeInput$1(System.Int32))("inp", "Output Count", 1)], Tesserae.INodeInput);
                }
            },
            Outputs: {
                get: function () {
                    return null;
                }
            }
        },
        alias: [
            "Name", "Tesserae$INodeView$Name",
            "Inputs", "Tesserae$INodeView$Inputs",
            "Outputs", "Tesserae$INodeView$Outputs",
            "UpdateNode", "Tesserae$IDynamicNodeView$UpdateNode"
        ],
        methods: {
            UpdateNode: function (inputs, outputs, addInput, addOutput) {
                var inputCount = H5.unbox(inputs.inp);
                for (var i = 0; i < inputCount; i = (i + 1) | 0) {
                    addOutput(new (Tesserae.NodeOutput$1(System.String))(System.String.format("out-{0}", [H5.box(i, System.Int32)]), System.String.format("out-{0}", [H5.box(i, System.Int32)]), H5.toString(i)));
                }
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.NodeViewSample.HelloWorldNode", {
        inherits: [Tesserae.INodeView],
        $kind: "nested class",
        props: {
            Name: {
                get: function () {
                    return "Hello World";
                }
            },
            Inputs: {
                get: function () {
                    return System.Array.init([new (Tesserae.NodeInput$1(System.String))("inp", "Input", "Hi Input")], Tesserae.INodeInput);
                }
            },
            Outputs: {
                get: function () {
                    return System.Array.init([new (Tesserae.NodeOutput$1(System.String))("out", "Output", "Hi Output")], Tesserae.INodeOutput);
                }
            }
        },
        alias: [
            "Name", "Tesserae$INodeView$Name",
            "Inputs", "Tesserae$INodeView$Inputs",
            "Outputs", "Tesserae$INodeView$Outputs"
        ]
    });

    H5.define("Tesserae.Tests.Samples.ObservableStackSample.StackElement", {
        inherits: [tss.ICID],
        $kind: "nested class",
        fields: {
            Id: null,
            DisplayName: null,
            Color: null
        },
        props: {
            Identifier: {
                get: function () {
                    return this.Id;
                }
            },
            ContentHash: {
                get: function () {
                    return H5.toString(Tesserae.Tests.Samples.HashingHelper.Fnv1aHash((this.Id || "") + (this.DisplayName || "")));
                }
            }
        },
        alias: [
            "Identifier", "tss$ICID$Identifier",
            "ContentHash", "tss$ICID$ContentHash",
            "Render", "tss$IC$Render"
        ],
        ctors: {
            ctor: function (id, displayName) {
                this.$initialize();
                this.Id = id;
                this.DisplayName = displayName;
                this.Color = String.fromCharCode(35) + (System.String.alignString(Math.floor(Math.random() * 16777215).toString(16), 6, 48) || "");
            }
        },
        methods: {
            Render: function () {
                return tss.UI.Card(tss.UI.Class(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Name").Inline().AutoWidth().SetContent(tss.UI.TextBlock(this.DisplayName)), tss.UI.Label$1("ID").Inline().AutoWidth().SetContent(tss.UI.TextBlock(this.Id)), tss.UI.Label$1("Hash").Inline().AutoWidth().SetContent(tss.ICX.Padding(tss.txt, tss.ISX.Background(tss.txt, tss.UI.TextBlock(this.ContentHash), this.Color), tss.usX.px$1(4)))]), "flash-bg")).Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PickerSampleItem", {
        inherits: [tss.IPickerItem],
        fields: {
            Name: null,
            IsSelected: false
        },
        alias: [
            "Name", "tss$IPickerItem$Name",
            "IsSelected", "tss$IPickerItem$IsSelected",
            "Render", "tss$IPickerItem$Render"
        ],
        ctors: {
            ctor: function (name) {
                this.$initialize();
                this.Name = name;
            }
        },
        methods: {
            Render: function () {
                return tss.UI.TextBlock(this.Name);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PickerSampleItemWithComponents", {
        inherits: [tss.IPickerItem],
        fields: {
            _icon: 0,
            Name: null,
            IsSelected: false
        },
        alias: [
            "Name", "tss$IPickerItem$Name",
            "IsSelected", "tss$IPickerItem$IsSelected",
            "Render", "tss$IPickerItem$Render"
        ],
        ctors: {
            ctor: function (name, icon) {
                this.$initialize();
                this.Name = name;
                this._icon = icon;
            }
        },
        methods: {
            Render: function () {
                return tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignContent("center"), [tss.ICX.MinWidth(tss.Icon, tss.UI.Icon$2(this._icon), tss.usX.px$1(16)), tss.ICX.ML(tss.txt, tss.UI.TextBlock(this.Name), 8)]);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ResourceCardSample.ModelItem", {
        inherits: [tss.ISearchableItem],
        $kind: "nested class",
        fields: {
            Title: null,
            Author: null,
            Capability: null,
            Description: null,
            Date: null,
            Icon: 0
        },
        alias: [
            "IsMatch", "tss$ISearchableItem$IsMatch",
            "Render", "tss$ISearchableItem$Render"
        ],
        ctors: {
            ctor: function (title, author, capability, description, date, icon) {
                this.$initialize();
                this.Title = title;
                this.Author = author;
                this.Capability = capability;
                this.Description = description;
                this.Date = date;
                this.Icon = icon;
            }
        },
        methods: {
            IsMatch: function (searchTerm) {
                if (System.String.isNullOrWhiteSpace(searchTerm)) {
                    return true;
                }
                searchTerm = searchTerm.toLowerCase();
                return System.String.contains(this.Title.toLowerCase(),searchTerm) || System.String.contains(this.Author.toLowerCase(),searchTerm) || System.String.contains(this.Capability.toLowerCase(),searchTerm) || System.String.contains(this.Description.toLowerCase(),searchTerm);
            },
            Render: function () {
                return tss.UI.ResourceCard().SetIcon(tss.UI.Icon$2(this.Icon, "fi-rr-", "tss-fontsize-large", void 0)).SetTitle(this.Title).SetSubtitle(this.Author).SetTags([tss.UI.Badge(this.Capability)]).SetDescription(this.Description).SetDate(this.Date).SetFooter(tss.UI.Link("https://example.com/terms", "Terms")).SetFooterCommands([tss.UI.Button$1("Copy ID").SetIcon$1(1303).NoBorder().NoBackground()]);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SamplesHelper", {
        statics: {
            methods: {
                SampleTitle$1: function (stack, sampleType, icon, subtitle) {
                    var text = Tesserae.Tests.Sample.FormatSampleName$1(sampleType);
                    return tss.SectionStackX.Title$1(stack, icon, text, subtitle, [tss.UI.Button$1("Documentation").SetIcon$1(535).OnClick$1(function () {
                        window.location.href = "https://docs.curiosity.ai/tesserae/";
                    }), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1("View Code").SetIcon$1(4301), "View source-code for this sample page").OnClick$1(function () {
                        Tesserae.Tests.Samples.SamplesHelper.ShowSampleCode(H5.Reflection.getTypeName(sampleType));
                    })]);
                },
                SampleTitle: function (stack, sampleType, icon, subtitle) {
                    var text = Tesserae.Tests.Sample.FormatSampleName(sampleType);
                    return tss.SectionStackX.Title$1(stack, icon, text, subtitle, [tss.UI.Button$1("Documentation").SetIcon$1(535).OnClick$1(function () {
                        window.location.href = "https://docs.curiosity.ai/tesserae/";
                    }), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1("View Code").SetIcon$1(4301), "View source-code for this sample page").OnClick$1(function () {
                        Tesserae.Tests.Samples.SamplesHelper.ShowSampleCode(sampleType);
                    })]);
                },
                ShowSampleCode: function (sampleType) {
                    var text = System.String.replaceAll(sampleType, "Sample", "");
                    tss.LayerExtensions.Content(tss.Modal, tss.ICX.W$1(tss.Modal, tss.UI.Modal((text || "") + " sample code").LightDismiss(), tss.usX.vh$1(80)).ShowCloseButton(), tss.ICX.H$1(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1(Tesserae.Tests.SamplesSourceCode.GetCodeForSample(sampleType))), tss.usX.vh$1(80))).Show();
                },
                SampleSubTitle: function (text) {
                    return tss.ICX.PB(tss.txt, tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(text)), 16), 8);
                },
                SampleDo: function (text) {
                    return tss.UI.Label$2(tss.ICX.PaddingRight(tss.Raw, tss.UI.Raw$2(tss.UI.I(tss.UI._$2("las la-check", void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                        s.color = "#107c10";
                    }))), tss.usX.px$1(8))).SetContent(tss.UI.TextBlock(text)).Inline();
                },
                SampleDont: function (text) {
                    return tss.UI.Label$2(tss.ICX.PaddingRight(tss.Raw, tss.UI.Raw$2(tss.UI.I(tss.UI._$2("las la-times", void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                        s.color = "#e81123";
                    }))), tss.usX.px$1(8))).SetContent(tss.UI.TextBlock(text)).Inline();
                }
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem", {
        inherits: [tss.ISearchableGroupedItem],
        $kind: "nested class",
        fields: {
            _value: null,
            _component: null,
            Group: null
        },
        alias: [
            "IsMatch", "tss$ISearchableItem$IsMatch",
            "Group", "tss$ISearchableGroupedItem$Group",
            "Render", "tss$ISearchableItem$Render"
        ],
        ctors: {
            ctor: function (value, group) {
                this.$initialize();
                this._value = value;
                this.Group = group;
                this._component = tss.ICX.Height(tss.Card, tss.UI.Card(tss.UI.TextBlock(value), true), tss.usX.px$1(64));
            }
        },
        methods: {
            IsMatch: function (searchTerm) {
                return System.String.contains(this._value.toLowerCase(),searchTerm.toLowerCase()) || System.String.contains(this.Group.toLowerCase(),searchTerm.toLowerCase());
            },
            Render: function () {
                return this._component;
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SearchableListSample.SearchableListItem", {
        inherits: [tss.ISearchableItem],
        $kind: "nested class",
        fields: {
            _value: null,
            _component: null
        },
        alias: ["IsMatch", "tss$ISearchableItem$IsMatch"],
        ctors: {
            ctor: function (value) {
                this.$initialize();
                this._value = value;
                this._component = tss.ICX.Height(tss.Card, tss.UI.Card(tss.UI.TextBlock(value), true), tss.usX.px$1(64));
            }
        },
        methods: {
            IsMatch: function (searchTerm) {
                return System.String.contains(this._value.toLowerCase(),searchTerm.toLowerCase());
            },
            Render: function () {
                return this._component.tss$IC$Render();
            },
            tss$ISearchableItem$Render: function () {
                return this._component;
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem", {
        inherits: function () { return [tss.IDetailsListItemT(Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem)]; },
        $kind: "nested class",
        statics: {
            fields: {
                Mapping: null
            },
            ctors: {
                init: function () {
                    this.Mapping = function (_o6) {
                            _o6.add("Default", function (_o1) {
                                    _o1.add("Background", tss.UI.Theme.Default.Background);
                                    _o1.add("Foreground", tss.UI.Theme.Default.Foreground);
                                    _o1.add("Border", tss.UI.Theme.Default.Border);
                                    _o1.add("BackgroundActive", tss.UI.Theme.Default.BackgroundActive);
                                    _o1.add("BackgroundHover", tss.UI.Theme.Default.BackgroundHover);
                                    _o1.add("ForegroundActive", tss.UI.Theme.Default.ForegroundActive);
                                    _o1.add("ForegroundHover", tss.UI.Theme.Default.ForegroundHover);
                                    return _o1;
                                }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor()));
                            _o6.add("Primary", function (_o2) {
                                    _o2.add("Background", tss.UI.Theme.Primary.Background);
                                    _o2.add("Foreground", tss.UI.Theme.Primary.Foreground);
                                    _o2.add("Border", tss.UI.Theme.Primary.Border);
                                    _o2.add("BackgroundActive", tss.UI.Theme.Primary.BackgroundActive);
                                    _o2.add("BackgroundHover", tss.UI.Theme.Primary.BackgroundHover);
                                    _o2.add("ForegroundActive", tss.UI.Theme.Primary.ForegroundActive);
                                    _o2.add("ForegroundHover", tss.UI.Theme.Primary.ForegroundHover);
                                    return _o2;
                                }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor()));
                            _o6.add("Secondary", function (_o3) {
                                    _o3.add("Background", tss.UI.Theme.Secondary.Background);
                                    _o3.add("Foreground", tss.UI.Theme.Secondary.Foreground);
                                    return _o3;
                                }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor()));
                            _o6.add("Danger", function (_o4) {
                                    _o4.add("Background", tss.UI.Theme.Danger.Background);
                                    _o4.add("Foreground", tss.UI.Theme.Danger.Foreground);
                                    _o4.add("Border", tss.UI.Theme.Danger.Border);
                                    _o4.add("BackgroundActive", tss.UI.Theme.Danger.BackgroundActive);
                                    _o4.add("BackgroundHover", tss.UI.Theme.Danger.BackgroundHover);
                                    _o4.add("ForegroundActive", tss.UI.Theme.Danger.ForegroundActive);
                                    _o4.add("ForegroundHover", tss.UI.Theme.Danger.ForegroundHover);
                                    return _o4;
                                }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor()));
                            _o6.add("Success", function (_o5) {
                                    _o5.add("Background", tss.UI.Theme.Success.Background);
                                    _o5.add("Foreground", tss.UI.Theme.Success.Foreground);
                                    _o5.add("Border", tss.UI.Theme.Success.Border);
                                    _o5.add("BackgroundActive", tss.UI.Theme.Success.BackgroundActive);
                                    _o5.add("BackgroundHover", tss.UI.Theme.Success.BackgroundHover);
                                    _o5.add("ForegroundActive", tss.UI.Theme.Success.ForegroundActive);
                                    _o5.add("ForegroundHover", tss.UI.Theme.Success.ForegroundHover);
                                    return _o5;
                                }(new (System.Collections.Generic.Dictionary$2(System.String,System.String)).ctor()));
                            return _o6;
                        }(new (System.Collections.Generic.Dictionary$2(System.String,System.Collections.Generic.Dictionary$2(System.String,System.String))).ctor());
                }
            },
            methods: {
                ColorSquare: function (color) {
                    if (System.String.isNullOrWhiteSpace(color)) {
                        return tss.UI.Raw$2(tss.UI.Div$2(tss.UI._$2(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                            s.width = "50px";
                            s.height = "49px";
                        })));
                    }

                    return tss.UI.Raw$2(tss.UI.Div$2(tss.UI._$2(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                        s.width = "50px";
                        s.height = "49px";
                        s.backgroundColor = color;
                        s.color = color;
                        s.borderColor = color;
                        s.boxShadow = "1px 1px 1px 1px lightgrey";
                    })));
                }
            }
        },
        fields: {
            ThemeName: null
        },
        props: {
            EnableOnListItemClickEvent: {
                get: function () {
                    return false;
                }
            }
        },
        alias: [
            "CompareTo", ["tss$IDetailsListItemT$Tesserae$Tests$Samples$ThemeColorsSample$ColorListItem$CompareTo", "tss$IDetailsListItemT$CompareTo"],
            "EnableOnListItemClickEvent", "tss$IDetailsListItem$EnableOnListItemClickEvent",
            "OnListItemClick", "tss$IDetailsListItem$OnListItemClick",
            "Render", "tss$IDetailsListItem$Render"
        ],
        ctors: {
            ctor: function (themeName) {
                this.$initialize();
                this.ThemeName = themeName;
            }
        },
        methods: {
            CompareTo: function (other, columnSortingKey) {
                return 0;
            },
            OnListItemClick: function (listItemIndex) {

                tss.UI.Toast().Information(H5.toString(listItemIndex));
            },
            Render: function (columns, createGridCellExpression) {
                return new (H5.GeneratorEnumerable$1(tss.IC))(H5.fn.bind(this, function (columns, createGridCellExpression) {
                    var $s = 0,
                        $jff,
                        $rv,
                        $ae;

                    var $en = new (H5.GeneratorEnumerator$1(tss.IC))(H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                switch ($s) {
                                    case 0: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 0, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(this.ThemeName);
                                            }));
                                            $s = 1;
                                            return true;
                                    }
                                    case 1: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 1, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.ColorSquare(System.Collections.Generic.CollectionExtensions.GetValueOrDefault(System.String, System.String, Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.Mapping.getItem(this.ThemeName), "Background", ""));
                                            }));
                                            $s = 2;
                                            return true;
                                    }
                                    case 2: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 2, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.ColorSquare(System.Collections.Generic.CollectionExtensions.GetValueOrDefault(System.String, System.String, Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.Mapping.getItem(this.ThemeName), "Foreground", ""));
                                            }));
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 3, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.ColorSquare(System.Collections.Generic.CollectionExtensions.GetValueOrDefault(System.String, System.String, Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.Mapping.getItem(this.ThemeName), "Border", ""));
                                            }));
                                            $s = 4;
                                            return true;
                                    }
                                    case 4: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 4, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.ColorSquare(System.Collections.Generic.CollectionExtensions.GetValueOrDefault(System.String, System.String, Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.Mapping.getItem(this.ThemeName), "BackgroundActive", ""));
                                            }));
                                            $s = 5;
                                            return true;
                                    }
                                    case 5: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 5, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.ColorSquare(System.Collections.Generic.CollectionExtensions.GetValueOrDefault(System.String, System.String, Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.Mapping.getItem(this.ThemeName), "BackgroundHover", ""));
                                            }));
                                            $s = 6;
                                            return true;
                                    }
                                    case 6: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 6, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.ColorSquare(System.Collections.Generic.CollectionExtensions.GetValueOrDefault(System.String, System.String, Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.Mapping.getItem(this.ThemeName), "ForegroundActive", ""));
                                            }));
                                            $s = 7;
                                            return true;
                                    }
                                    case 7: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 7, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.ColorSquare(System.Collections.Generic.CollectionExtensions.GetValueOrDefault(System.String, System.String, Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem.Mapping.getItem(this.ThemeName), "ForegroundHover", ""));
                                            }));
                                            $s = 8;
                                            return true;
                                    }
                                    case 8: {

                                    }
                                    default: {
                                        return false;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            throw $ae;
                        }
                    }));
                    return $en;
                }, arguments));
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.UIconsSample.IconItem", {
        inherits: [tss.ISearchableItem],
        $kind: "nested class",
        fields: {
            _value: null,
            component: null
        },
        alias: [
            "IsMatch", "tss$ISearchableItem$IsMatch",
            "Render", "tss$ISearchableItem$Render"
        ],
        ctors: {
            ctor: function (icon, name) {
                this.$initialize();
                name = Tesserae.Tests.Samples.UIconsSample.ToValidName(name.substr(6));
                this._value = (name || "") + " " + (System.Enum.toString(Tesserae.UIcons, icon) || "");

                this.component = tss.ICTX.Children$6(tss.S, tss.ICX.PB(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()).AlignItemsCenter(), 4), [tss.ICX.MinWidth(tss.Icon, tss.UI.Icon$2(icon, "fi-rr-", "tss-fontsize-large", void 0), tss.usX.px$1(36)), tss.ICX.Grow(tss.txt, tss.ICX.W(tss.txt, tss.txtX.Title(tss.txt, tss.txtX.Ellipsis(tss.txt, tss.UI.TextBlock(System.String.format("{0}", [name]))), System.Enum.toString(Tesserae.UIcons, icon)), 1))]);

            }
        },
        methods: {
            IsMatch: function (searchTerm) {
                return System.String.contains(this._value,searchTerm);
            },
            Render: function () {
                return this.component;
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem", {
        inherits: [tss.IC],
        $kind: "nested class",
        fields: {
            _innerElement: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function (text) {
                this.$initialize();
                this._innerElement = tss.UI.Div$2(tss.UI._$2(void 0, void 0, void 0, void 0, void 0, void 0, text, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                    s.display = "flex";
                    s.alignItems = "center";
                    s.padding = "0 16px";
                    s.height = "40px";
                    s.borderBottom = "1px solid #eee";
                }));
            }
        },
        methods: {
            Render: function () {
                return this._innerElement;
            }
        }
    });

    H5.define("Tesserae.Tests.SamplesSourceCode", {
        statics: {
            methods: {
                GetCodeForSample: function (sampleName) {
                    switch (sampleName) {
                        default: 
                            return "Missing sample code";
                    }
                }
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.AccordionSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.AccordionSample, 151, "A collapsible content component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("An accordion contains a list of expanders that can be toggled to reveal more information. They are useful for organizing content into manageable chunks and reducing vertical space usage when not all information needs to be visible at once."), tss.UI.TextBlock("Tesserae's Accordion component manages multiple Expanders, allowing you to control whether one or multiple sections can be open at the same time.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use accordions to organize related content that might be too long to display all at once. Ensure the header of each expander clearly describes the content within. Avoid nesting accordions within accordions as it can lead to confusion. Consider using a single Expander if you only have one block of optional content.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Accordion"), tss.UI.Accordion([tss.UI.Expander("Getting started", tss.UI.TextBlock("Use expanders to reveal details in place without navigating away.")).Expanded(), tss.UI.Expander("Configuration", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.TextBlock("You can nest any component inside an expander."), tss.UI.Button$1("Primary action").Primary()])), tss.UI.Expander("Advanced", tss.UI.TextBlock("Combine with SectionStack or Card for complex layouts."))]).AllowMultipleOpen(false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Accordion with Multiple Open Allowed"), tss.UI.Accordion([tss.UI.Expander("Section 1", tss.UI.TextBlock("Multiple sections can be open simultaneously here.")), tss.UI.Expander("Section 2", tss.UI.TextBlock("This is useful for comparing information between sections.")), tss.UI.Expander("Section 3", tss.UI.TextBlock("Just set .AllowMultipleOpen(true) on the accordion."))]).AllowMultipleOpen(true), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standalone Expander"), tss.UI.Expander("What is Tesserae?", tss.UI.TextBlock("Tesserae provides a fluent API for building UI components.")).Expanded(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Expander with OptionIcon and ChevronRight"), tss.UI.Accordion([tss.UI.Expander("What does indexing do?", tss.UI.TextBlock("Indexing reads each document, extracts structured text, and stores it in a vector + keyword index so queries match in milliseconds.\n\nSource files stay where they are \u2014 only metadata leaves your machine.")).OptionIcon(2520, tss.UI.Theme.Colors.Blue600, tss.UI.Theme.Colors.Blue100).ChevronRight().Expanded(), tss.UI.Expander("Where are my files stored?", tss.UI.TextBlock("")).OptionIcon(964, tss.UI.Theme.Colors.Green600, tss.UI.Theme.Colors.Green100).ChevronRight(), tss.UI.Expander("Why did my last build fail?", tss.UI.TextBlock("")).OptionIcon(1737, tss.UI.Theme.Colors.Orange600, tss.UI.Theme.Colors.Orange100).ChevronRight(), tss.UI.Expander("How do I rotate the IMAP token?", tss.UI.TextBlock("")).OptionIcon(4709, tss.UI.Theme.Colors.Red600, tss.UI.Theme.Colors.Red100).ChevronRight(), tss.UI.Expander("Can I use a custom embedding model?", tss.UI.TextBlock("")).OptionIcon(3974, tss.UI.Theme.Colors.Blue600, tss.UI.Theme.Colors.Blue100).ChevronRight()]).AllowMultipleOpen(false)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ActionButtonSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var btn1 = { };
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ActionButtonSample, 1378, "A button that triggers an action"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ActionButtons are a variation of the standard Button component that split the interaction into two distinct parts: a display area (typically the label and an icon) and a specific action area (typically a secondary icon on the right)."), tss.UI.TextBlock("They are useful when you want to provide a primary action while also offering a secondary, related action like opening a menu, showing a tooltip, or triggering a specific sub-task.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ActionButtons when a component needs to perform more than one related task. The primary area should trigger the most common action, while the secondary area (the action icon) should trigger a complementary one. Clearly distinguish between the two areas visually if they perform very different tasks. Ensure that both interaction points have appropriate tooltips or labels if their purpose isn't immediately obvious.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Action Buttons"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Var(tss.ActionButton, tss.UI.ActionButton("Standard Action"), btn1).OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Information("Clicked display!");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Information("Clicked action icon!");
                }), tss.UI.ActionButton("Primary with Calendar Icon", 768, "fi-rr-", void 0, "tss-fontsize-small").Primary().OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Success("Main area clicked");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Success("Calendar icon clicked");
                }), tss.UI.ActionButton$1("Danger Action", 4709, "fi-rr-", void 0, "tss-fontsize-small", "fi-rr-", 108, void 0, "tss-fontsize-small").Danger().OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Error("Danger area clicked");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Error("Warning icon clicked");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Complex Content"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.ActionButton$2(tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.UI.Icon$2(243), tss.ICX.PL(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Move Item")), 8)]), tss.ICX.PT(tss.txt, tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("Use this to reorganize your workspace")), 4)])).OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Information("Moving...");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Information("Configure move...");
                }), tss.UI.ActionButton("Action with Custom Tooltip").ModifyActionButton(function (btn) {
                    tss.ICX.Tooltip$1(tss.Raw, tss.UI.Raw$3(btn), "This tooltip is applied to the entire component");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dropdown Simulation"), tss.UI.ActionButton("Show Options", 118, "fi-rr-", void 0, "tss-fontsize-small").Primary().OnClickAction(function (s, e) {
                    var hideAction = { v : null };
                    var menu = tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Button$1("Option 1").OnClick$1(function () {
                        tss.UI.Toast().Information("Option 1");
                        !H5.staticEquals(hideAction.v, null) ? hideAction.v() : null;
                    }), tss.UI.Button$1("Option 2").OnClick$1(function () {
                        tss.UI.Toast().Information("Option 2");
                        !H5.staticEquals(hideAction.v, null) ? hideAction.v() : null;
                    }), tss.UI.Button$1("Option 3").OnClick$1(function () {
                        tss.UI.Toast().Information("Option 3");
                        !H5.staticEquals(hideAction.v, null) ? hideAction.v() : null;
                    })]).Render();
                    tss.tippy.ShowFor(s, menu, hideAction, "none", "bottom-end", 0, 0, 350, true, null);
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.AnnotatedTextEditorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            fields: {
                SampleText: null,
                _vocabulary: null
            },
            ctors: {
                init: function () {
                    this.SampleText = "Curiosity GmbH, based in Munich, Germany, is the company behind Tesserae.\r\nIn January 2026, Alice and Bob met in San Francisco to discuss a $1.5 billion partnership between Anthropic and OpenAI.\r\nClaude and GPT-4 are large language models. Jules built a demo using Tesserae for Microsoft in 2025.";
                    this._vocabulary = System.Array.init([new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Curiosity GmbH", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Curiosity", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Anthropic", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("OpenAI", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Microsoft", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Tesserae", "PRODUCT", "var(--tss-colors-blue-100)", "var(--tss-colors-blue-900)", "var(--tss-colors-blue-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Claude", "PRODUCT", "var(--tss-colors-blue-100)", "var(--tss-colors-blue-900)", "var(--tss-colors-blue-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("GPT-4", "PRODUCT", "var(--tss-colors-blue-100)", "var(--tss-colors-blue-900)", "var(--tss-colors-blue-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Berlin", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Munich", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("San Francisco", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Germany", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Europe", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Alice", "PERSON", "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Bob", "PERSON", "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("Jules", "PERSON", "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("2024", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("2025", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("2026", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("January", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("$1.5 billion", "MONEY", "var(--tss-colors-yellow-100)", "var(--tss-colors-yellow-900)", "var(--tss-colors-yellow-500)"), new (System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).$ctor1("$200", "MONEY", "var(--tss-colors-yellow-100)", "var(--tss-colors-yellow-900)", "var(--tss-colors-yellow-500)")], System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String));
                }
            },
            methods: {
                AnnotateAsync: function (text) {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            var $t;
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(150)));

                            if (System.String.isNullOrEmpty(text)) {
                                return System.Array.init(0, null, tss.AnnotatedTextEditor.Entity);
                            }

                            var found = new (System.Collections.Generic.List$1(tss.AnnotatedTextEditor.Entity)).ctor();

                            $t = H5.getEnumerator(System.Linq.Enumerable.from(Tesserae.Tests.Samples.AnnotatedTextEditorSample._vocabulary, System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)).orderByDescending(function (v) {
                                    return v.Item1.length;
                                }));
                            try {
                                while ($t.moveNext()) {
                                    var v = $t.Current.$clone();
                                    var idx = 0;

                                    while (idx < text.length) {
                                        var found_idx = { v : System.String.indexOf(text, v.Item1, idx, null, 5) };
                                        if (found_idx.v < 0) {
                                            break;
                                        }

                                        var end = { v : (found_idx.v + v.Item1.length) | 0 };
                                        var overlaps = System.Linq.Enumerable.from(found, tss.AnnotatedTextEditor.Entity).any((function ($me, found_idx, end) {
                                                return function (e) {
                                                    return found_idx.v < e.End && end.v > e.Start;
                                                };
                                            })(this, found_idx, end));

                                        if (!overlaps) {
                                            found.add(new tss.AnnotatedTextEditor.Entity.$ctor1(found_idx.v, v.Item1.length, v.Item2, v.Item3, v.Item4, v.Item5));
                                        }
                                        idx = end.v;
                                    }
                                }
                            } finally {
                                if (H5.is($t, System.IDisposable)) {
                                    $t.System$IDisposable$Dispose();
                                }
                            }

                            return System.Linq.Enumerable.from(found, tss.AnnotatedTextEditor.Entity).orderBy(function (e) {
                                    return e.Start;
                                }).ToArray(tss.AnnotatedTextEditor.Entity);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                },
                AnnotateAllTokensAsync: function (text) {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            var $t, $t1;
                            var colored = (await H5.toPromise(Tesserae.Tests.Samples.AnnotatedTextEditorSample.AnnotateAsync(text)));

                            if (System.String.isNullOrEmpty(text)) {
                                return colored;
                            }

                            var all = new (System.Collections.Generic.List$1(tss.AnnotatedTextEditor.Entity)).$ctor1(colored);

                            var i = 0;
                            while (i < text.length) {
                                if (System.Char.isWhiteSpace(String.fromCharCode(text.charCodeAt(i)))) {
                                    i = (i + 1) | 0;
                                    continue;
                                }

                                var start = { v : i };

                                if (($t = text.charCodeAt(i), (System.Char.isDigit($t) || System.Char.isLetter($t)))) {
                                    while (i < text.length && ($t1 = text.charCodeAt(i), (System.Char.isDigit($t1) || System.Char.isLetter($t1)))) {
                                        i = (i + 1) | 0;
                                    }
                                } else {
                                    i = (i + 1) | 0;
                                }

                                var end = { v : i };

                                var overlapsColored = System.Linq.Enumerable.from(colored, tss.AnnotatedTextEditor.Entity).any((function ($me, start, end) {
                                        return function (e) {
                                            return start.v < e.End && end.v > e.Start;
                                        };
                                    })(this, start, end));
                                if (!overlapsColored) {
                                    all.add(new tss.AnnotatedTextEditor.Entity.$ctor1(start.v, ((end.v - start.v) | 0), "TOKEN", "var(--tss-colors-neutral-200)", "var(--tss-colors-neutral-900)", "var(--tss-colors-neutral-500)"));
                                }
                            }

                            return System.Linq.Enumerable.from(all, tss.AnnotatedTextEditor.Entity).orderBy(function (e) {
                                    return e.Start;
                                }).ToArray(tss.AnnotatedTextEditor.Entity);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var entityCountLabel = tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("0 entities")), tss.UI.Theme.Secondary.Foreground);

                var editor = tss.UI.AnnotatedTextEditor(Tesserae.Tests.Samples.AnnotatedTextEditorSample.AnnotateAsync, "Curiosity GmbH, based in Munich, Germany, is the company behind Tesserae.\r\nIn January 2026, Alice and Bob met in San Francisco to discuss a $1.5 billion partnership between Anthropic and OpenAI.\r\nClaude and GPT-4 are large language models. Jules built a demo using Tesserae for Microsoft in 2025.\nCuriosity GmbH, based in Munich, Germany, is the company behind Tesserae.\r\nIn January 2026, Alice and Bob met in San Francisco to discuss a $1.5 billion partnership between Anthropic and OpenAI.\r\nClaude and GPT-4 are large language models. Jules built a demo using Tesserae for Microsoft in 2025.\n", 500, "Type some text and entities will be highlighted automatically...").MinHeight(tss.usX.px$1(160)).OnAnnotationsChanged(function (s, entities) {
                    entityCountLabel.Text = entities.length === 1 ? "1 entity" : System.String.format("{0} entities", [H5.box(entities.length, System.Int32)]);
                }).OnEntityClick(function (s, entity, e) {
                    tss.UI.Toast().Information(System.String.format("Clicked entity: \"{0}\" ({1})", s.Text.substr(entity.Start, entity.Length), entity.Label));
                });

                var allTokensEditor = tss.UI.AnnotatedTextEditor(Tesserae.Tests.Samples.AnnotatedTextEditorSample.AnnotateAllTokensAsync, Tesserae.Tests.Samples.AnnotatedTextEditorSample.SampleText, 500, void 0).MinHeight(tss.usX.px$1(120)).OnEntityClick(function (s, entity, e) {
                    tss.UI.Toast().Information(System.String.format("Token: \"{0}\" ({1})", s.Text.substr(entity.Start, entity.Length), entity.Label));
                });

                var readOnlyEditor = tss.UI.AnnotatedTextEditor(Tesserae.Tests.Samples.AnnotatedTextEditorSample.AnnotateAsync, "In 2025, Alice and Bob visited Munich to demo Tesserae for Curiosity GmbH.", 500, void 0).MinHeight(tss.usX.px$1(60)).ReadOnly().OnEntityClick(function (s, entity, e) {
                    tss.UI.Toast().Success(System.String.format("{0}: {1}", entity.Label, s.Text.substr(entity.Start, entity.Length)));
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.AnnotatedTextEditorSample, 2380, "Annotated text editor"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("AnnotatedTextEditor is a multi-line editable text field that highlights NLP entities in place, like the OmniBox token rendering. A debounced async lambda (default 500ms) is called after the user stops typing and returns the entities found in the text."), tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.ICX.MT(tss.txt, tss.UI.TextBlock("Try editing the text below \u2014 entities are re-detected after you pause typing."), 8)), tss.UI.Theme.Secondary.Foreground)])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Multi-line, editable, with entity highlighting"), tss.ICX.MB(tss.txt, entityCountLabel, 8), tss.ICX.WS(tss.AnnotatedTextEditor, editor), tss.ICX.MT(tss.IC, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Every token annotated, matched ones colored"), 16), tss.ICX.MB(tss.txt, tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Every word is wrapped in a gray pill, and the vocabulary matches override the gray with their typed color.")), tss.UI.Theme.Secondary.Foreground), 8), tss.ICX.WS(tss.AnnotatedTextEditor, allTokensEditor), tss.ICX.MT(tss.IC, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Read-only with clickable entities"), 16), tss.ICX.MB(tss.txt, tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Text cannot be edited, but entities still react to clicks.")), tss.UI.Theme.Secondary.Foreground), 8), tss.ICX.WS(tss.AnnotatedTextEditor, readOnlyEditor)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.AvatarSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.AvatarSample, 4799, "A component that represents a user"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Avatars are used to represent users, teams, or entities in the system. They can display images, initials, and presence indicators."), tss.UI.TextBlock("The Persona component builds upon Avatar by adding textual information like name, role, and status, making it ideal for profile cards or contact lists.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use avatars to provide visual recognition for users. Always provide initials as a fallback for when images fail to load or aren't available. Use the appropriate size for the context\u2014smaller for lists or chat, larger for profiles. Presence indicators should be used when real-time availability information is relevant to the user's task.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Avatar Sizes and Presence"), tss.UI.TextBlock("Avatars support various sizes from XSmall to XLarge and optional presence states."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.XSmall), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.Small).Presence(Tesserae.AvatarPresence.Online), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.Medium).Presence(Tesserae.AvatarPresence.Away), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.Large).Presence(Tesserae.AvatarPresence.Busy), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.XLarge).Presence(Tesserae.AvatarPresence.Offline)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Initials Fallback"), tss.UI.TextBlock("When no image is provided, initials are displayed with a generated background color. The background color is a deterministic hash-based gradient generated from the initials."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Avatar(void 0, "JD").Size(Tesserae.AvatarSize.Small).Presence(Tesserae.AvatarPresence.Online), tss.UI.Avatar(void 0, "AS").Size(Tesserae.AvatarSize.Medium).Presence(Tesserae.AvatarPresence.Away), tss.UI.Avatar(void 0, "KL").Size(Tesserae.AvatarSize.Large).Presence(Tesserae.AvatarPresence.Busy), tss.UI.Avatar(void 0, "MW").Size(Tesserae.AvatarSize.XLarge).Presence(Tesserae.AvatarPresence.Offline)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Persona Component"), tss.UI.TextBlock("Personas combine an avatar with descriptive text."), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Persona("Jordan Diaz", "Product Designer", "Available for collaboration", tss.UI.Avatar(void 0, "JD").Presence(Tesserae.AvatarPresence.Online)), tss.UI.Persona("Alex Smith", "Software Engineer", "Focusing...", tss.UI.Avatar(void 0, "AS").Presence(Tesserae.AvatarPresence.Busy)), tss.UI.Persona("Kelly Lee", "Project Manager", "Away", tss.UI.Avatar(void 0, "KL").Presence(Tesserae.AvatarPresence.Away))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.BadgeSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.BadgeSample, 909, "A component to display a badge"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Badges, Tags, and Chips are small visual elements used to categorize content, highlight status, or display metadata."), tss.UI.TextBlock("They come in various styles: Badges are typically static indicators, Tags are for categorization, and Chips often include interactive elements like a removal button.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use badges to call attention to small pieces of information like counts or status. Use tags for categorization where multiple labels might apply. Use chips for entities that can be removed or interacted with individually. Ensure colors are used consistently to convey meaning (e.g., red for danger/errors, green for success).")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standard Badges"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Badge("Default"), tss.UI.Badge("Primary").Primary(), tss.UI.Badge("Success").Success(), tss.UI.Badge("Warning").Warning(), tss.UI.Badge("Danger").Danger(), tss.UI.Badge("Info").Info().Outline()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Tags and Chips"), tss.UI.TextBlock("Tags and chips support icons, pill shapes, and interactive removal."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Tag("Categorization").Outline().Pill(), tss.UI.Tag("Metadata").SetIcon(tss.Icon.Transform(4462, "fi-rr-")).Outline(), tss.UI.Chip("Interactive Chip").Filled().OnRemove(function (c) {
                    tss.UI.Toast().Success("Removed chip");
                }), tss.UI.Chip("Status Chip").Success().Pill()])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.BreadcrumbSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.BreadcrumbSample, 120, "A breadcrumb navigation component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Breadcrumbs provide a secondary navigation system that reveals a user's location in a website or web app. They allow for one-click access to any higher level in the hierarchy."), tss.UI.TextBlock("Unlike TextBreadcrumbs, this component supports more advanced configuration like custom chevrons, overflow indices, and different sizes.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Place breadcrumbs at the top of the page, above the primary content. Use them when the site hierarchy is at least two levels deep. Each breadcrumb item should represent a page or a container. The last item should represent the current location and be non-clickable. Ensure that the breadcrumbs collapse gracefully on smaller screens or when space is limited.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Breadcrumbs"), tss.ICX.PB(tss.Breadcrumb, tss.UI.Breadcrumb().Items([tss.UI.Crumb("Home").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Home");
                }), tss.UI.Crumb("Project A").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Project A");
                }), tss.UI.Crumb("Subfolder 1").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Subfolder 1");
                }), tss.UI.Crumb("Current Page")]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Responsive and Collapsed"), tss.UI.TextBlock("Breadcrumbs will collapse when the container width is restricted."), tss.ICX.PB(tss.Breadcrumb, tss.ICX.MaxWidth(tss.Breadcrumb, tss.UI.Breadcrumb(), tss.usX.px$1(250)).Items([tss.UI.Crumb("Root"), tss.UI.Crumb("Level 1"), tss.UI.Crumb("Level 2"), tss.UI.Crumb("Level 3"), tss.UI.Crumb("Final")]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Small Size and Custom Chevron"), tss.ICX.PB(tss.Breadcrumb, tss.UI.Breadcrumb().Small().SetChevron(120).Items([tss.UI.Crumb("Resources"), tss.UI.Crumb("Icons"), tss.UI.Crumb("UIcons")]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Overflow Configuration"), tss.UI.TextBlock("You can control where the overflow starts (e.g., after the second item)."), tss.ICX.MaxWidth(tss.Breadcrumb, tss.UI.Breadcrumb().SetOverflowIndex(1), tss.usX.px$1(200)).Items([tss.UI.Crumb("Home"), tss.UI.Crumb("App"), tss.UI.Crumb("Module"), tss.UI.Crumb("Feature"), tss.UI.Crumb("Detail")])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ButtonSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ButtonSample, 1378, "A control that triggers an action"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog."), tss.UI.TextBlock("When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Buttons should clearly communicate what will happen when the user clicks them. Use concise, specific, self-explanatory labels, usually a single word. Default buttons should always perform safe operations. For example, a default button should never delete. Use only a single line of text in the label of the button. Expose only one or two buttons to the user at a time. Show only one primary button that inherits theme color at rest state. \"Submit\", \"OK\", and \"Apply\" buttons should always be styled as primary buttons."), tss.UI.TextBlock("Avoid using generic labels like \"Ok\", especially in the case of an error. Do not place the default focus on a button that destroys data. Do not use a button to navigate to another place, use a link instead.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Buttons"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetText("Standard"), "This is a standard button").OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetText("Primary"), "This is a primary button").Primary().OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetText("Link"), "This is a link button").Link().OnClick$1(function () {
                    alert("Clicked!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Icons and States"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Confirm").SetIcon$1(964).Success().OnClick$1(function () {
                    alert("Clicked!");
                }), tss.UI.Button$1().SetText("Delete").SetIcon$1(4675).Danger().OnClick$1(function () {
                    alert("Clicked!");
                }), tss.UI.Button$1().SetText("Disabled").SetIcon$1(2802).Disabled()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Loading States"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Click to Spin").OnClickSpinWhile(function () {
                    var $tcs = new H5.TCS();
                    (async () => (await H5.toPromise(System.Threading.Tasks.Task.delay(2000))))().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }), tss.UI.Button$1().SetText("With Loading Text").OnClickSpinWhile(function () {
                    var $tcs = new H5.TCS();
                    (async () => (await H5.toPromise(System.Threading.Tasks.Task.delay(2000))))().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }, "Processing..."), tss.UI.Button$1().SetText("Error Simulation").OnClickSpinWhile(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(1000)));
                            throw new System.Exception("Action failed");
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }, void 0, function (b, e) {
                    b.SetText("Try again: " + (e.Message || "")).Danger();
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Variations"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.ButtonAndIcon("Split Button", function (m, i, ev) {
                    tss.UI.Toast().Information("Icon clicked");
                }, 3802, 118).OnClick(function (b, _) {
                    tss.UI.Toast().Success("Main action");
                }), tss.UI.Button$1().SetText("No Padding").NoPadding().Primary(), tss.UI.Button$1().SetText("No Border").NoBorder()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Themed Backgrounds"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Blue"), tss.UI.Theme.Colors.Blue500).OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Lime"), tss.UI.Theme.Colors.Lime500).OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Magenta"), tss.UI.Theme.Colors.Magenta500).OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Yellow"), tss.UI.Theme.Colors.Yellow500).OnClick$1(function () {
                    alert("Clicked!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Buttons"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ISX.Rounded(tss.Button, tss.UI.Button$1().SetText("Small"), tss.BR.Small).Primary(), tss.ISX.Rounded(tss.Button, tss.UI.Button$1().SetText("Medium"), tss.BR.Medium).Primary(), tss.ISX.Rounded(tss.Button, tss.UI.Button$1().SetText("Full"), tss.BR.Full).Primary()])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.CardPivotSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.CardPivotSample, 151, "A pivot using cards"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A CardPivot is a tabbed interface where the tabs are presented as connected cards with a shared border. It is useful for displaying selectable metrics that control the view below them.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Usage"), tss.CardPivotX.CardPivot(tss.CardPivotX.CardPivot(tss.CardPivotX.CardPivot(tss.CardPivotX.CardPivot(tss.CardPivotX.CardPivot(tss.UI.CardPivot(), "tab1", function () {
                    return tss.UI.Metric("Requests", "1.1k").Change(tss.ISX.Foreground(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("")), tss.UI.Theme.Colors.Neutral600));
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Requests"), 32));
                }), "tab2", function () {
                    return tss.UI.Metric("Tokens", "196.97k");
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tokens"), 32));
                }), "tab3", function () {
                    return tss.UI.Metric("Cost", "$0.09");
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Cost"), 32));
                }), "tab4", function () {
                    return tss.UI.Metric("Errors", "194");
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Errors"), 32));
                }), "tab5", function () {
                    return tss.UI.Metric("Cached", "408");
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Cached"), 32));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.CardSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.CardSample, 48, "A card component with optional headers and footers"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Cards are surfaces that display content and actions on a single topic."), tss.UI.TextBlock("They should be easy to scan for relevant and actionable information. Elements, like text and images, should be placed on them in a way that clearly indicates hierarchy."), tss.UI.TextBlock("Cards can contain different types of components. They can be used to show a list of items, a single item, or a mix of both.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.UI.Stack().Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use cards to group related information."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Keep the information on a card concise."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use clear, concise, and easy to understand language.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use cards to display unrelated information."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't overload a card with too much information."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use cards to display a list of items.")])])])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Basic Card")), 8), tss.UI.Card(tss.UI.TextBlock("This is a basic card.")), tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Card with Header")), 16), tss.UI.Card(tss.UI.TextBlock("This is a card with a header.")).SetTitle$1(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()).AlignItemsCenter(), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Header")), tss.ICX.ML(tss.Tag, tss.UI.Tag("Sample Card").Primary(), 8)])), tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Card with Header and Footer")), 16), tss.UI.Card(tss.UI.TextBlock("This is a card with a header and a footer.")).SetTitle("Header").SetFooter(tss.UI.Button$1("Action").Primary()), tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Compact Card")), 16), tss.UI.Card(tss.UI.TextBlock("This is a compact card.")).SetTitle("Header").Compact(), tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Hover Card")), 16), tss.UI.Card(tss.UI.TextBlock("This card has hover effect.")).HoverColor(), tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Custom Background")), 16), tss.UI.Card(tss.UI.TextBlock("This card has a custom background.")).BackgroundColor(tss.UI.Theme.Primary.Background)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.CarouselSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var textSlide1 = tss.ICX.WS(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.P(tss.S, tss.UI.VStack(), 32), [tss.ITFX.Bold(tss.txt, tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Discover Tesserae"))), tss.UI.TextBlock("Build stunning user interfaces in C# that compile to high-performance JavaScript.")]));
                var textSlide2 = tss.ICX.WS(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.P(tss.S, tss.UI.VStack(), 32), [tss.ITFX.Bold(tss.txt, tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Fluent API"))), tss.UI.TextBlock("Leverage a powerful, typed, and fluent API to define your layout and logic efficiently.")]));
                var textSlide3 = tss.ICX.WS(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.P(tss.S, tss.UI.VStack(), 32), [tss.ITFX.Bold(tss.txt, tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Native Performance"))), tss.UI.TextBlock("Zero-overhead abstractions mean your application runs fast on any modern browser.")]));

                var imgSlide1 = tss.ICX.H(tss.Image, tss.ICX.WS(tss.Image, tss.UI.Image$1("https://cataas.com/cat")), 300).Contain();
                var imgSlide2 = tss.ICX.H(tss.Image, tss.ICX.WS(tss.Image, tss.UI.Image$1("https://cataas.com/cat")), 300).Contain();
                var imgSlide3 = tss.ICX.H(tss.Image, tss.ICX.WS(tss.Image, tss.UI.Image$1("https://cataas.com/cat")), 300).Contain();

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.CarouselSample, 3460, "A slideshow component for cycling through elements"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Carousels allow users to cycle through a set of related content, such as images, features, or messages. They are effective for showcasing highlights in a limited space."), tss.UI.TextBlock("The component supports any Tesserae component as a slide and provides automatic or manual navigation.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use carousels for high-impact visual content. Keep the number of slides low (typically 3-5) to ensure users can reasonably see all content. Ensure that each slide has a clear and unique message. Provide navigation controls (arrows/dots) and ensure they are accessible. For slides with text content, ensure sufficient contrast and use .PadSlides() to prevent overlapping with controls.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Text Carousel"), tss.ICX.H(tss.Carousel, tss.UI.Carousel([textSlide1, textSlide2, textSlide3]).PadSlides(), 150), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Image Gallery Carousel"), tss.ICX.H(tss.Carousel, tss.UI.Carousel([imgSlide1, imgSlide2, imgSlide3]), 300), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Carousel"), tss.ICX.H(tss.Carousel, tss.UI.Carousel([tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Interactive Slide")), tss.UI.Button$1("Click me").OnClick$1(function () {
                    tss.UI.Toast().Success("Clicked!");
                })]), 32), tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Configuration Slide")), tss.UI.CheckBox$1("Enable feature")]), 32)]).PadSlides(), 150)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ChatSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                var $t;
                this.$initialize();
                var AddAIAnswer = null;
                var chatArea = tss.UI.ChatArea();

                var predefinedAnswers = System.Array.init(["That's a very interesting question. Let me think about it for a second...", "I completely agree with you on that point. Here's some more context.", "Here are a few ways we could tackle this problem. First, we could...',", "I am not entirely sure, but based on the available data...", "Excellent! Let's proceed with the proposed plan."], System.String);

                var random = new System.Random.ctor();

                var cancelled = false;

                var input = null;



                chatArea.Add(tss.UI.ChatMessage(tss.UI.TextBlock("Hello there!"), tss.UI.Avatar(null, "U")).RightAligned().MaxWidth());
                chatArea.Add(tss.UI.ChatMessage(tss.UI.TextBlock("Hi! How can I help you today?"), tss.UI.Avatar(null, "AI")).MaxWidth());

                chatArea.Add(tss.UI.ChatMessage(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.ToolCall(1749, "Read /home/user/project/src/App.tsx", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("import React from 'react';\n\nexport default function App() {\n    return <div>Hello</div>;\n}"));
                }), tss.ICX.PT(tss.txt, tss.UI.TextBlock("I just read App.tsx \u2014 it's a minimal React component. Want me to add routing?"), 8)]), tss.UI.Avatar(null, "AI")).MaxWidth());

                chatArea.Add(tss.UI.ChatMessage(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.ToolsUsed([tss.UI.ToolCall(4510, "Bash ls -la && git status", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("On branch main\nnothing to commit, working tree clean"));
                }), tss.UI.ToolCall(1749, "Read /home/user/project/README.md", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("# Project\n\nA sample app."));
                }), tss.UI.ToolCall(3936, "Grep \"TODO\" src/", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("src/App.tsx:14: // TODO: add routing"));
                }), tss.UI.ToolCall(4510, "Bash dotnet build", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("Build succeeded.\n    0 Warning(s)\n    0 Error(s)"));
                }), tss.UI.ToolCall(1749, "Read /home/user/project/src/index.tsx", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("import { createRoot } from 'react-dom/client';\n..."));
                }), tss.UI.ToolCall(4615, "ToolSearch routing", function () {
                    return tss.UI.TextBlock("Found: react-router-dom v6");
                }), tss.UI.ToolCall(2774, "Update todos").NotExpandable()]).SetSummary("Ran 4 commands, read 2 files, used a tool"), tss.ICX.PT(tss.txt, tss.UI.TextBlock("I scanned the project, ran the build, and looked at routing options. Here's what I found:"), 8)]), tss.UI.Avatar(null, "AI")).MaxWidth());
                AddAIAnswer = function () {
                    var TypeNextWord = null;
                    cancelled = false;
                    var answer = predefinedAnswers[System.Array.index(random.Next$1(predefinedAnswers.length), predefinedAnswers)];
                    var words = System.String.split(answer, [32].map(function (i) {{ return String.fromCharCode(i); }}));
                    var copyButton = tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$2(1303).NoBorder().NoBackground(), "Copy");
                    copyButton.Render().classList.remove("tss-btn-default");
                    copyButton.Render().classList.add("tss-btn-icon-only");
                    var msgComponent = tss.UI.ChatMessage(tss.UI.TextBlock(""), tss.UI.Avatar(null, "AI"), copyButton).MaxWidth();
                    chatArea.Add(msgComponent);
                    var index = 0;
                    var currentText = "";


                    TypeNextWord = function () {
                        if (cancelled) {
                            input.IsGenerating = false;
                            return;
                        }

                        if (index >= words.length) {
                            input.IsGenerating = false;
                            return;
                        }

                        currentText = (currentText || "") + ((((index > 0 ? " " : "") || "") + (words[System.Array.index(index, words)] || "")) || "");
                        msgComponent.ReplaceContent(tss.UI.TextBlock(currentText));
                        msgComponent.KeepVisible();
                        index = (index + 1) | 0;
                        window.setTimeout(function (_) {
                            TypeNextWord();
                        }, 150);
                    };
                    window.setTimeout(function (_) {
                        TypeNextWord();
                    }, 500);
                };

                input = tss.UI.OmniBox(($t = new tss.OmniBox.Config(tss.OmniBox.Mode.Chat), $t.PlaceholderChat = "Ask anything...", $t.IconStop = 4354, $t.IconChat = 208, $t)).OnChat(function (sender, msg) {
                    var text = msg.Text;
                    if (System.String.isNullOrWhiteSpace(text)) {
                        return;
                    }

                    chatArea.Add(tss.UI.ChatMessage(tss.UI.TextBlock(text), tss.UI.Avatar(null, "U")).RightAligned().MaxWidth());

                    sender.IsGenerating = true;
                    AddAIAnswer();
                }).OnStop(function (sender) {
                    cancelled = true;
                    sender.IsGenerating = false;
                });

                var chatContainer = tss.ICTX.Children$6(tss.S, tss.ICX.Grow(tss.S, tss.ICX.H(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), 10)), [tss.ICX.Grow(tss.ChatArea, tss.ICX.H(tss.ChatArea, tss.ICX.WS(tss.ChatArea, chatArea), 10)), tss.ICX.WS(tss.OmniBox, input).H(150)]);

                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ChatSample, 1256, "A component to display a chat"), tss.ICX.S(tss.Card, tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ChatArea and ChatMessage components allow building modern chat experiences with dynamic, animatable messages using DeltaComponent."), tss.ICX.MT(tss.S, chatContainer, 16)])).SetTitle("Overview")), true, false, "");
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.CheckBoxSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.CheckBoxSample, 969, "A control that allows the user to select multiple options"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("CheckBoxes allow users to select one or more items from a set. They can also be used to turn an option on or off."), tss.UI.TextBlock("Unlike a Toggle, which is typically used for immediate actions, a CheckBox is often used when a user needs to confirm their selection by clicking a submit button.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use CheckBoxes when users can select any number of options from a list. Clearly label each CheckBox so the user knows what they are selecting. If you have only two mutually exclusive options, consider using a ChoiceGroup (Radio buttons) or a Toggle. Don't use CheckBoxes as an on/off control for immediate actions; use a Toggle instead.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic CheckBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.CheckBox$1("Unchecked checkbox"), tss.UI.CheckBox$1("Checked checkbox").Checked(), tss.UI.CheckBox$1("Disabled checkbox").Disabled(), tss.UI.CheckBox$1("Disabled checked checkbox").Checked().Disabled()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation and States"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Required(tss.Label, tss.UI.Label$1("Required choice")).SetContent(tss.UI.CheckBox$1("I agree to the terms")), tss.ICX.Tooltip$1(tss.ChecBox, tss.UI.CheckBox$1("Checkbox with tooltip"), "More info here"), tss.UI.CheckBox$1("Triggers event").OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Checked: {0}", [H5.box(s.IsChecked, System.Boolean, System.Boolean.toString)]));
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Formatting"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Tiny(tss.ChecBox, tss.UI.CheckBox$1("Tiny")), tss.ITFX.Small(tss.ChecBox, tss.UI.CheckBox$1("Small (default)")), tss.ITFX.SmallPlus(tss.ChecBox, tss.UI.CheckBox$1("Small Plus")), tss.ITFX.Medium(tss.ChecBox, tss.UI.CheckBox$1("Medium")), tss.ITFX.Large(tss.ChecBox, tss.UI.CheckBox$1("Large")), tss.ITFX.XLarge(tss.ChecBox, tss.UI.CheckBox$1("XLarge")), tss.ITFX.XXLarge(tss.ChecBox, tss.UI.CheckBox$1("XXLarge")), tss.ITFX.Mega(tss.ChecBox, tss.UI.CheckBox$1("Mega")), tss.ITFX.Bold(tss.ChecBox, tss.UI.CheckBox$1("Bold text"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded CheckBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.ChecBox, tss.UI.CheckBox$1("Small rounded"), tss.BR.Small), tss.ISX.Rounded(tss.ChecBox, tss.UI.CheckBox$1("Medium rounded"), tss.BR.Medium), tss.ISX.Rounded(tss.ChecBox, tss.UI.CheckBox$1("Full rounded"), tss.BR.Full)])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ChoiceGroupSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ChoiceGroupSample, 2773, "A control to select a single option from a list"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ChoiceGroups, also known as radio button groups, allow users to select exactly one option from a set of mutually exclusive choices."), tss.UI.TextBlock("They emphasize all options equally, which can be useful when you want to ensure the user considers all available alternatives before making a selection.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ChoiceGroups when there are between 2 and 7 options and screen space is available. For more than 7 options, a Dropdown is typically more efficient. List options in a logical order (e.g., most likely to least likely). Align options vertically whenever possible for better readability and localization support. Always provide a default selection if one is significantly more likely than the others, and ensure the safest option is the default if applicable.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic ChoiceGroup"), tss.UI.ChoiceGroup().Choices([tss.UI.Choice("Option A"), tss.UI.Choice("Option B"), tss.UI.Choice("Option C").Disabled(), tss.UI.Choice("Option D")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Required with Label"), tss.UI.ChoiceGroup("Select an environment").Required().Choices([tss.UI.Choice("Development"), tss.UI.Choice("Staging"), tss.UI.Choice("Production")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Horizontal Layout"), tss.UI.ChoiceGroup("Sizes").Horizontal().Choices([tss.UI.Choice("Small"), tss.ITFX.Medium(tss.ChoiceGroup.Choice, tss.UI.Choice("Medium")), tss.ITFX.Large(tss.ChoiceGroup.Choice, tss.UI.Choice("Large"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.ChoiceGroup("Language").Choices([tss.UI.Choice("English"), tss.UI.Choice("Spanish"), tss.UI.Choice("French")]).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}", [s.SelectedOption.Text]));
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Formatting"), tss.UI.ChoiceGroup("Pick a style").Choices([tss.ITFX.Tiny(tss.ChoiceGroup.Choice, tss.UI.Choice("Tiny")), tss.ITFX.Small(tss.ChoiceGroup.Choice, tss.UI.Choice("Small (default)")), tss.ITFX.SmallPlus(tss.ChoiceGroup.Choice, tss.UI.Choice("Small Plus")), tss.ITFX.Medium(tss.ChoiceGroup.Choice, tss.UI.Choice("Medium")), tss.ITFX.Large(tss.ChoiceGroup.Choice, tss.UI.Choice("Large")), tss.ITFX.XLarge(tss.ChoiceGroup.Choice, tss.UI.Choice("XLarge")), tss.ITFX.XXLarge(tss.ChoiceGroup.Choice, tss.UI.Choice("XXLarge")), tss.ITFX.Mega(tss.ChoiceGroup.Choice, tss.UI.Choice("Mega")), tss.ITFX.Bold(tss.ChoiceGroup.Choice, tss.UI.Choice("Bold"))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ColorPaletteSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var selectedColor = new (tss.SettableObservableT(System.String))("#0078d4");

                var palette = tss.UI.ColorPalette().Swatches([tss.ColorPalette.Define("Blue", "#0078d4"), tss.ColorPalette.Define("Purple", "#8764b8"), tss.ColorPalette.Define("Magenta", "#e3008c"), tss.ColorPalette.Define("Red", "#d13438"), tss.ColorPalette.Define("Orange", "#ca5010"), tss.ColorPalette.Define("Yellow", "#ffaa44"), tss.ColorPalette.Define("Green", "#107c10"), tss.ColorPalette.Define("Teal", "#038387"), tss.ColorPalette.Define("Neutral", "#737373"), tss.ColorPalette.Define("Black", "#000000")]).SetValue("#0078d4").WithCustomColor().OnChange$1(function (c) {
                    selectedColor.Value$1 = c;
                    tss.UI.Toast().Information(System.String.format("Selected: {0}", [c]));
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ColorPaletteSample, 3262, "A grid of named colour swatches"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ColorPalette displays a set of named swatches for picking from a predefined brand or theme palette. It's distinct from the raw ColorPicker which allows any colour \u2014 use ColorPalette when the set of valid choices is known in advance.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Define swatches using your design system's named tokens. Always include an accessible label for each swatch. If custom colours are allowed, add WithCustomColor().")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Brand Palette"), palette, tss.ICTX.Children$6(tss.S, tss.ICX.MT(tss.S, tss.UI.HStack(), 12).AlignItems("center"), [tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Selected:")), tss.UI.DeferSync$3(System.String, selectedColor, function (c) {
                    var swatch = tss.UI.Span(tss.UI._());
                    swatch.style.display = "inline-block";
                    swatch.style.width = "20px";
                    swatch.style.height = "20px";
                    swatch.style.background = c;
                    swatch.style.borderRadius = "50%";
                    swatch.style.border = "1px solid rgba(0,0,0,0.15)";
                    return tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(8)), [tss.UI.Raw$2(swatch), tss.ITFX.Small(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(c)))]);
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Semantic Colours"), tss.UI.ColorPalette().Swatches([tss.ColorPalette.Define("Success", tss.UI.Theme.Success.Background), tss.ColorPalette.Define("Primary", tss.UI.Theme.Primary.Background), tss.ColorPalette.Define("Danger", tss.UI.Theme.Danger.Background), tss.ColorPalette.Define("Info", tss.UI.Theme.Primary.Background)]).OnChange$1(function (c) {
                    tss.UI.Toast().Information(System.String.format("Semantic color: {0}", [c]));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ColorPickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var cp1 = { };
                var btn1 = { };
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ColorPickerSample, 3262, "A control to pick a color"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The ColorPicker allows users to select a color using the browser's native color selection widget. It returns the selected color as both a hex string and a Color object."), tss.UI.TextBlock("This component is useful for personalization settings, drawing applications, or any interface where color customization is required.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use the ColorPicker when users need to select a precise color that isn't covered by a predefined set of options. If you only need a few specific colors, consider using a ChoiceGroup with custom styling or a Dropdown instead. Always provide a default color that makes sense for the context. Ensure the picked color is validated if certain constraints apply (e.g., must be a dark color for text readability).")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic ColorPicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Pick a color").SetContent(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.ColorPicker, tss.UI.Var(tss.ColorPicker, tss.UI.ColorPicker(), cp1), tss.usX.px$1(50)), tss.UI.Var(tss.Button, tss.UI.Button$1("Apply Color"), btn1)])), tss.UI.Label$1("With default color (Blue)").SetContent(tss.ICX.Width(tss.ColorPicker, tss.UI.ColorPicker(tss.Color.FromString("#0078d4")), tss.usX.px$1(50))), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled state")).SetContent(tss.ICX.Width(tss.ColorPicker, tss.UI.ColorPicker().Disabled(), tss.usX.px$1(50)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Light color required").SetContent(tss.ICX.Width(tss.ColorPicker, tss.vX.Validation(tss.ColorPicker, tss.UI.ColorPicker(), tss.Validation.LightColor), tss.usX.px$1(50))), tss.UI.Label$1("Dark color required").SetContent(tss.ICX.Width(tss.ColorPicker, tss.vX.Validation(tss.ColorPicker, tss.UI.ColorPicker(), tss.Validation.DarkColor), tss.usX.px$1(50)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Example"), tss.UI.TextBlock("Changing the color picker below will update the button's background.")])).SetTitle("Usage")]));

                cp1.v.OnChange(function (_, __) {
                    btn1.v.Background = cp1.v.Text;
                });
                btn1.v.OnClick(function (_, __) {
                    tss.UI.Toast().Information(System.String.format("Selected Color: {0} (Hex: {1})", cp1.v.Text, cp1.v.Color.ToHex()));
                });
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ColorsSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var Render = null;
                var allColors = System.Array.init([new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime100", tss.UI.Theme.Colors.Lime100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime200", tss.UI.Theme.Colors.Lime200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime250", tss.UI.Theme.Colors.Lime250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime300", tss.UI.Theme.Colors.Lime300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime400", tss.UI.Theme.Colors.Lime400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime500", tss.UI.Theme.Colors.Lime500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime600", tss.UI.Theme.Colors.Lime600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime700", tss.UI.Theme.Colors.Lime700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime800", tss.UI.Theme.Colors.Lime800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime850", tss.UI.Theme.Colors.Lime850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime900", tss.UI.Theme.Colors.Lime900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime1000", tss.UI.Theme.Colors.Lime1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red100", tss.UI.Theme.Colors.Red100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red200", tss.UI.Theme.Colors.Red200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red250", tss.UI.Theme.Colors.Red250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red300", tss.UI.Theme.Colors.Red300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red400", tss.UI.Theme.Colors.Red400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red500", tss.UI.Theme.Colors.Red500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red600", tss.UI.Theme.Colors.Red600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red700", tss.UI.Theme.Colors.Red700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red800", tss.UI.Theme.Colors.Red800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red850", tss.UI.Theme.Colors.Red850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red900", tss.UI.Theme.Colors.Red900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red1000", tss.UI.Theme.Colors.Red1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange100", tss.UI.Theme.Colors.Orange100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange200", tss.UI.Theme.Colors.Orange200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange250", tss.UI.Theme.Colors.Orange250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange300", tss.UI.Theme.Colors.Orange300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange400", tss.UI.Theme.Colors.Orange400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange500", tss.UI.Theme.Colors.Orange500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange600", tss.UI.Theme.Colors.Orange600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange700", tss.UI.Theme.Colors.Orange700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange800", tss.UI.Theme.Colors.Orange800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange850", tss.UI.Theme.Colors.Orange850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange900", tss.UI.Theme.Colors.Orange900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange1000", tss.UI.Theme.Colors.Orange1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow100", tss.UI.Theme.Colors.Yellow100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow200", tss.UI.Theme.Colors.Yellow200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow250", tss.UI.Theme.Colors.Yellow250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow300", tss.UI.Theme.Colors.Yellow300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow400", tss.UI.Theme.Colors.Yellow400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow500", tss.UI.Theme.Colors.Yellow500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow600", tss.UI.Theme.Colors.Yellow600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow700", tss.UI.Theme.Colors.Yellow700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow800", tss.UI.Theme.Colors.Yellow800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow850", tss.UI.Theme.Colors.Yellow850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow900", tss.UI.Theme.Colors.Yellow900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow1000", tss.UI.Theme.Colors.Yellow1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green100", tss.UI.Theme.Colors.Green100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green200", tss.UI.Theme.Colors.Green200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green250", tss.UI.Theme.Colors.Green250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green300", tss.UI.Theme.Colors.Green300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green400", tss.UI.Theme.Colors.Green400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green500", tss.UI.Theme.Colors.Green500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green600", tss.UI.Theme.Colors.Green600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green700", tss.UI.Theme.Colors.Green700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green800", tss.UI.Theme.Colors.Green800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green850", tss.UI.Theme.Colors.Green850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green900", tss.UI.Theme.Colors.Green900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green1000", tss.UI.Theme.Colors.Green1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal100", tss.UI.Theme.Colors.Teal100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal200", tss.UI.Theme.Colors.Teal200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal250", tss.UI.Theme.Colors.Teal250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal300", tss.UI.Theme.Colors.Teal300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal400", tss.UI.Theme.Colors.Teal400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal500", tss.UI.Theme.Colors.Teal500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal600", tss.UI.Theme.Colors.Teal600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal700", tss.UI.Theme.Colors.Teal700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal800", tss.UI.Theme.Colors.Teal800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal850", tss.UI.Theme.Colors.Teal850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal900", tss.UI.Theme.Colors.Teal900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal1000", tss.UI.Theme.Colors.Teal1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue100", tss.UI.Theme.Colors.Blue100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue200", tss.UI.Theme.Colors.Blue200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue250", tss.UI.Theme.Colors.Blue250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue300", tss.UI.Theme.Colors.Blue300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue400", tss.UI.Theme.Colors.Blue400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue500", tss.UI.Theme.Colors.Blue500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue600", tss.UI.Theme.Colors.Blue600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue700", tss.UI.Theme.Colors.Blue700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue800", tss.UI.Theme.Colors.Blue800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue850", tss.UI.Theme.Colors.Blue850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue900", tss.UI.Theme.Colors.Blue900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue1000", tss.UI.Theme.Colors.Blue1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple100", tss.UI.Theme.Colors.Purple100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple200", tss.UI.Theme.Colors.Purple200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple250", tss.UI.Theme.Colors.Purple250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple300", tss.UI.Theme.Colors.Purple300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple400", tss.UI.Theme.Colors.Purple400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple500", tss.UI.Theme.Colors.Purple500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple600", tss.UI.Theme.Colors.Purple600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple700", tss.UI.Theme.Colors.Purple700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple800", tss.UI.Theme.Colors.Purple800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple850", tss.UI.Theme.Colors.Purple850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple900", tss.UI.Theme.Colors.Purple900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple1000", tss.UI.Theme.Colors.Purple1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta100", tss.UI.Theme.Colors.Magenta100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta200", tss.UI.Theme.Colors.Magenta200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta250", tss.UI.Theme.Colors.Magenta250), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta300", tss.UI.Theme.Colors.Magenta300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta400", tss.UI.Theme.Colors.Magenta400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta500", tss.UI.Theme.Colors.Magenta500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta600", tss.UI.Theme.Colors.Magenta600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta700", tss.UI.Theme.Colors.Magenta700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta800", tss.UI.Theme.Colors.Magenta800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta850", tss.UI.Theme.Colors.Magenta850), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta900", tss.UI.Theme.Colors.Magenta900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta1000", tss.UI.Theme.Colors.Magenta1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral0", tss.UI.Theme.Colors.Neutral0), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral100", tss.UI.Theme.Colors.Neutral100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral200", tss.UI.Theme.Colors.Neutral200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral300", tss.UI.Theme.Colors.Neutral300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral400", tss.UI.Theme.Colors.Neutral400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral500", tss.UI.Theme.Colors.Neutral500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral600", tss.UI.Theme.Colors.Neutral600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral700", tss.UI.Theme.Colors.Neutral700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral800", tss.UI.Theme.Colors.Neutral800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral900", tss.UI.Theme.Colors.Neutral900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral1000", tss.UI.Theme.Colors.Neutral1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Neutral1100", tss.UI.Theme.Colors.Neutral1100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral0", tss.UI.Theme.Colors.DarkNeutral0), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral100", tss.UI.Theme.Colors.DarkNeutral100), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral200", tss.UI.Theme.Colors.DarkNeutral200), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral300", tss.UI.Theme.Colors.DarkNeutral300), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral400", tss.UI.Theme.Colors.DarkNeutral400), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral500", tss.UI.Theme.Colors.DarkNeutral500), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral600", tss.UI.Theme.Colors.DarkNeutral600), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral700", tss.UI.Theme.Colors.DarkNeutral700), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral800", tss.UI.Theme.Colors.DarkNeutral800), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral900", tss.UI.Theme.Colors.DarkNeutral900), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral1000", tss.UI.Theme.Colors.DarkNeutral1000), new (System.ValueTuple$2(System.String,System.String)).$ctor1("DarkNeutral1100", tss.UI.Theme.Colors.DarkNeutral1100)], System.ValueTuple$2(System.String,System.String));



                var groups = System.Linq.Enumerable.from(allColors, System.ValueTuple$2(System.String,System.String)).groupBy(H5.fn.cacheBind(this, this.GetGroupName));

                var grid = tss.UI.Grid([tss.usX.fr$1(1), tss.usX.fr$1(1), tss.usX.fr$1(1)]).Gap(tss.usX.px$1(8));


                Render = H5.fn.bind(this, function () {
                    tss.ICTX.Children$6(tss.Grid, grid, groups.orderBy(function (g) {
                        return H5.rE(g.key(), "Neutral") ? 0 : H5.rE(g.key(), "DarkNeutral") ? 1 : 2;
                    }).thenBy(function (g) {
                        return g.key();
                    }).select(H5.fn.bind(this, function (g) {
                        return tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle(g.key()), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), g.select(H5.fn.bind(this, function (c) {
                            return this.RenderColorStack(c.Item1, c.Item2);
                        })).ToArray(tss.IC))]);
                    })).ToArray(tss.S));
                });
                Render();

                tss.UI.Theme.addOnThemeChanged(function () {
                    window.setTimeout(function (_) {
                        Render();
                    }, 1);
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ColorsSample, 3262, "A utility to apply colors"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Tesserae provides a comprehensive set of predefined colors that are part of the theme. These colors are accessible via the 'Theme.Colors' class and are designed to provide a consistent visual language across the application, with support for both light and dark modes.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Prefer using these predefined colors over hardcoded hex values to ensure your application remains consistent with the theme. Use specific color ranges (e.g., Red for errors, Green for success) to convey semantic meaning. Click on any color name below to copy its C# constant name, or use the icons to copy its RGB or Hex values.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [grid])).SetTitle("Usage")]));
            }
        },
        methods: {
            GetGroupName: function (color) {
                if (System.String.startsWith(color.Item1, "DarkNeutral")) {
                    return "DarkNeutral";
                }
                if (System.String.startsWith(color.Item1, "Neutral")) {
                    return "Neutral";
                }

                return System.String.fromCharArray(System.Linq.Enumerable.from(color.Item1, System.Char).takeWhile(System.Char.isLetter).ToArray(System.Char));
            },
            RenderColorStack: function (colorName, colorVar) {
                var color = tss.Color.FromString(colorVar);
                var hsl = new tss.HSLColor.$ctor2(color);
                var textColor = "black";
                if (hsl.Luminosity < 100) {
                    textColor = "white";
                }
                return tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.UI.HStack().NoWrap(), colorVar), [tss.ICX.Grow(tss.Button, tss.ICX.W(tss.Button, tss.ISX.Foreground(tss.Button, tss.UI.Button$1(colorName), textColor).NoBackground(), 10)).OnClick$1(function () {
                    tss.Clipboard.Copy(System.String.format("Theme.Colors.{0}", [colorName]));
                }), tss.ICX.Tooltip$1(tss.Button, tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetIcon$1(1303, textColor, "tss-fontsize-small", "fi-rr-", false), color.ToRGB()).OnClick$1(function () {
                    tss.Clipboard.Copy(color.ToRGB());
                }), System.String.format("Copy RGB Value: {0}", [color.ToRGB()])), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetIcon$1(2323, textColor, "tss-fontsize-small", "fi-rr-", false).OnClick$1(function () {
                    tss.Clipboard.Copy(color.ToHex());
                }), System.String.format("Copy Hex Value: {0}", [color.ToHex()]))])]), 8);
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.CommandBarSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.CommandBarSample, 2943, "A toolbar for housing commands"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Command Bars provide a surface for common actions related to a specific context, such as a page or a selected item in a list."), tss.UI.TextBlock("They typically contain buttons with icons and labels, and can be split into 'near' items (left-aligned) and 'far' items (right-aligned).")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Command Bars for primary actions that users perform frequently. Keep the number of items manageable; if there are too many, consider using a 'More' menu. Order items by importance or frequency of use. Group related actions together. Use 'far' items for actions that are global to the surface, such as settings or search.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Command Bar"), tss.UI.CommandBar([tss.UI.CommandBarItem("New", 3536).Primary().OnClick$1(function () {
                    tss.UI.Toast().Success("New item");
                }), tss.UI.CommandBarItem("Edit", 1663).OnClick$1(function () {
                    tss.UI.Toast().Success("Edit item");
                }), tss.UI.CommandBarItem("Share", 3981).OnClick$1(function () {
                    tss.UI.Toast().Success("Share item");
                }), tss.UI.CommandBarItem("Delete", 4675).OnClick$1(function () {
                    tss.UI.Toast().Success("Delete item");
                })]).FarItems([tss.ICX.Width(tss.SearchBox, tss.UI.SearchBox().SetPlaceholder("Search..."), tss.usX.px$1(200)), tss.UI.CommandBarItem("Settings", 3974).OnClick$1(function () {
                    tss.UI.Toast().Information("Settings clicked");
                })])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.CommandPaletteSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                BuildActions: function () {
                    var $t;
                    var navigate = new tss.CommandPaletteAction("navigation", "Navigate");
                    var home = ($t = new tss.CommandPaletteAction("home", "Go to Home"), $t.ParentId = "navigation", $t.Perform = function () {
                        tss.UI.Toast().Success("Home");
                    }, $t);
                    var settings = ($t = new tss.CommandPaletteAction("settings", "Open Settings"), $t.ParentId = "navigation", $t.Perform = function () {
                        tss.UI.Toast().Success("Settings");
                    }, $t);
                    var help = ($t = new tss.CommandPaletteAction("help", "Help Center"), $t.Perform = function () {
                        tss.UI.Toast().Success("Help");
                    }, $t.Shortcut = System.Array.init(["?"], System.String), $t.Keywords = "support docs", $t.Icon = 1258, $t);
                    var create = ($t = new tss.CommandPaletteAction("new", "Create Item"), $t.Perform = function () {
                        tss.UI.Toast().Success("Create");
                    }, $t.Shortcut = System.Array.init(["n"], System.String), $t.Section = "Actions", $t.Icon = 3536, $t);
                    var archive = ($t = new tss.CommandPaletteAction("archive", "Archive Item"), $t.Perform = function () {
                        tss.UI.Toast().Success("Archive");
                    }, $t.Section = "Actions", $t.Icon = 159, $t);

                    return System.Array.init([navigate, home, settings, help, create, archive], tss.CommandPaletteAction);
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var stack = tss.UI.SectionStack().Secondary();
                var palette = new tss.CommandPalette(stack, Tesserae.Tests.Samples.CommandPaletteSample.BuildActions());

                var openButton = tss.UI.Button$1("Open Command Palette").Primary().OnClick$1(function () {
                    palette.Open();
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(stack, Tesserae.Tests.Samples.CommandPaletteSample, 2612, "A command palette utility"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("CommandPalette provides a fast and efficient way for users to navigate an application and trigger commands using only their keyboard. Inspired by modern editors and tools, it allows users to search through a list of actions and execute them with a single keystroke.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Register all major application actions in the CommandPalette. Use intuitive shortcuts and keywords to make actions easy to discover. Organize related actions into sections and utilize hierarchies for a cleaner interface. Ensure that common global actions are always easily accessible via the palette.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.PB(tss.txt, tss.txtX.Secondary(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Use the button below or press Cmd/Ctrl + K to open the palette."))), 8), openButton, tss.ICX.PT(tss.txt, tss.txtX.Secondary(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Try navigating with arrow keys, Enter, Esc, and Backspace for nested items."))), 12)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ContextMenuSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var btn1 = { };
                var btn2 = { };
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ContextMenuSample, 1378, "A menu that appears on context"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ContextMenu is a flyout component that displays a list of commands triggered by user interaction, such as a right-click or a button press."), tss.UI.TextBlock("It provides a focused set of actions relevant to the current context, helping to keep the main interface clean and uncluttered. It supports nested submenus, dividers, headers, and custom component items.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ContextMenus to surface secondary actions that are relevant to a specific element. Group related commands using dividers. Use submenus sparingly to avoid deep nesting that can be hard to navigate. Ensure that the menu remains within the viewport when opened. Always provide clear labels and icons for common actions.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Simple Context Menu"), tss.ICX.MB(tss.Button, tss.UI.Var(tss.Button, tss.UI.Button$1("Click for Menu"), btn1).OnClick(function (s, e) {
                    tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(3536), tss.ICX.ML(tss.txt, tss.UI.TextBlock("New Item"), 8)])).OnClick(function (_, __) {
                        tss.UI.Toast().Success("New Item created");
                    }), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(2020), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Open"), 8)])).OnClick(function (_, __) {
                        tss.UI.Toast().Information("Opening...");
                    }), tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$1(4675, tss.UI.Theme.Danger.Background), tss.txtX.Danger(tss.txt, tss.ICX.ML(tss.txt, tss.UI.TextBlock("Delete"), 8))])).OnClick(function (_, __) {
                        tss.UI.Toast().Error("Deleted");
                    })]).ShowFor$1(btn1.v);
                }), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Menu with Submenus and Headers"), tss.UI.Var(tss.Button, tss.UI.Button$1("Complex Menu"), btn2).OnClick(function (s, e) {
                    tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem("Actions").Header(), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(1663), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Edit"), 8)])).SubMenu(tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem("Edit Name"), tss.UI.ContextMenuItem("Edit Permissions"), tss.UI.ContextMenuItem("Edit Metadata")])), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(3981), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Share"), 8)])).SubMenu(tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem("Copy Link"), tss.UI.ContextMenuItem("Email Link")])), tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem("Advanced").Header(), tss.UI.ContextMenuItem("Properties").Disabled(), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(3974), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Settings"), 8)]))]).ShowFor$1(btn2.v);
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.CronEditorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.CronEditorSample, 1110, "A component to edit cron expressions"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("CronEditor allows users to schedule tasks using a simplified UI for daily schedules, with a fallback to raw cron expressions for advanced users.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic"), tss.UI.CronEditor(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Days Selection Disabled"), tss.UI.CronEditor().DaysEnabled(false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Interval (30 mins)"), tss.UI.CronEditor().MinuteInterval(30), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Initial Value (Custom)"), tss.UI.CronEditor("*/5 * * * *"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Initially Disabled"), tss.UI.CronEditor("0 12 * * *", false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Enable Checkbox Hidden"), tss.UI.CronEditor().ShowEnableCheckbox(false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Observable"), this.GetObservableExample()])).SetTitle("Usage")]));
            }
        },
        methods: {
            GetObservableExample: function () {
                var editor = tss.UI.CronEditor();
                var text = tss.UI.TextBlock("Current value: " + (editor.Value.Item1 || "") + " (" + ((editor.Value.Item2 ? "Enabled" : "Disabled") || "") + ")");
                editor.OnChange$1(function (s) {
                    text.Text = "Current value: " + (s.Value.Item1 || "") + " (" + ((s.Value.Item2 ? "Enabled" : "Disabled") || "") + ")";
                });
                return tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [editor, text]);
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DatePickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var from = System.DateTime.addDays(System.DateTime.getNow(), -7);
                var to = System.DateTime.addDays(System.DateTime.getNow(), 7);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.DatePickerSample, 768, "A component to select a date"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("DatePickers allow users to select a specific date using a browser-native date selection widget. They ensure that the input is always a valid date format."), tss.UI.TextBlock("This component is suitable for forms requiring birthdays, appointment dates, or any date-driven data entry.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use the DatePicker when users need to enter a specific date. If you need to include time as well, use the DateTimePicker instead. Always provide min and max constraints if the acceptable date range is limited. Use clear validation messages to guide users if they select an invalid date (e.g., a date in the past when only future dates are allowed).")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic DatePicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.DatePicker()), tss.UI.Label$1("Pre-selected Date (Next Week)").SetContent(tss.UI.DatePicker(System.DateTime.addDays(System.DateTime.getNow(), 7))), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.DatePicker().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Range and Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1(System.String.format("Limited Range (Between {0:d} and {1:d})", H5.box(from, System.DateTime, System.DateTime.format), H5.box(to, System.DateTime, System.DateTime.format))).SetContent(tss.UI.DatePicker().SetMin(from).SetMax(to)), tss.UI.Label$1("Step increment of 5 days").SetContent(tss.UI.DatePicker().SetStep(5))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Not in the future").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), tss.Validation.NotInTheFuture)), tss.UI.Label$1("Not in the past").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), tss.Validation.NotInThePast)), tss.UI.Label$1("Custom validation (within 2 months)").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), function (dp) {
                    return System.DateTime.lte(dp.Date, System.DateTime.addMonths(System.DateTime.getNow(), 2)) ? null : "Please choose a date less than 2 months in the future";
                }))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.DatePicker().OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected date: {0:d}", [H5.box(s.Date, System.DateTime, System.DateTime.format)]));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DateTimePickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var from = System.DateTime.addDays(System.DateTime.getNow(), -7);
                var to = System.DateTime.addDays(System.DateTime.getNow(), 7);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.DateTimePickerSample, 768, "A control to pick a date and time"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The DateTimePicker combines date and time selection into a single component, using the browser's native widget."), tss.UI.TextBlock("It is ideal for scheduling events, setting deadlines, or any task where both the day and time are critical.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use the DateTimePicker when users need to specify a precise moment in time. Consider the user's timezone if the application handles users across different regions. Provide sensible defaults, such as the current time or a common starting point. Use min/max constraints to prevent invalid selections (e.g., booking an appointment in the past).")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic DateTimePicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.DateTimePicker()), tss.UI.Label$1("Pre-selected (Now)").SetContent(tss.UI.DateTimePicker(System.DateTime.getNow())), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.DateTimePicker().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1(System.String.format("Range: {0:g} to {1:g}", H5.box(from, System.DateTime, System.DateTime.format), H5.box(to, System.DateTime, System.DateTime.format))).SetContent(tss.UI.DateTimePicker().SetMin(from).SetMax(to)), tss.UI.Label$1("10-second intervals").SetContent(tss.UI.DateTimePicker().SetStep(10))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Must be in the future").SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), tss.Validation.NotInThePast$1)), tss.UI.Label$1("Within next 48 hours").SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), function (dtp) {
                    return System.DateTime.lte(dtp.DateTime, System.DateTime.addHours(System.DateTime.getNow(), 48)) ? null : "Must be within 48 hours";
                }))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.DateTimePicker().OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0:g}", [H5.box(s.DateTime, System.DateTime, System.DateTime.format)]));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DeferSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var stack = tss.UI.SectionStack().Secondary();
                var countSlider = tss.UI.Slider(5, 0, 10, 1);

                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.DeferSample, 1110, "A utility to defer execution"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Defer is a powerful utility for handling asynchronous UI rendering. It allows you to wrap a component that depends on an async task (like a network request). While the task is in progress, a loading message or a skeleton loader can be shown. Once the task completes, the actual component is seamlessly rendered in its place.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Defer for any component that requires remote data or slow computations. Provide a 'loadMessage'\u2014ideally a skeleton loader\u2014that mimics the final layout to reduce visual flickering. Leverage the lazy-loading nature of Defer, as the async task is only triggered when the component is first rendered. Combine Defer with observables to create highly reactive and responsive interfaces.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Number of items:").SetContent(countSlider.OnInput(H5.fn.bind(this, function (s, e) {
                    this.SetChildren(stack, s.Value);
                })))])])]), tss.ICX.HeightAuto(tss.SectionStack, stack)])).SetTitle("Usage")]));
                this.SetChildren(stack, 5);
            }
        },
        methods: {
            SetChildren: function (stack, count) {
                stack.Clear();

                for (var i = 0; i < count; i = (i + 1) | 0) {
                    var delay = { v : H5.Int.mul((((i + 1) | 0)), 1000) };

                    tss.SectionStackX.FlatSection(stack, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ITFX.SemiBold(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock(System.String.format("Section {0} - delayed {1} seconds", H5.box(i, System.Int32), H5.box(((i + 1) | 0), System.Int32))))), tss.UI.Defer$1((function ($me, delay) {
                        return H5.fn.bind($me, function () {
                            var $tcs = new H5.TCS();
                            (async () => {
                                {
                                    (await H5.toPromise(System.Threading.Tasks.Task.delay(delay.v)));

                                    return tss.ICTX.Children$6(tss.S, tss.ICX.HS(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack())), [tss.ICX.H(tss.Image, tss.ICX.W(tss.Image, tss.UI.Image$1("./assets/img/curiosity-logo.svg"), 40), 40), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.ICX.W(tss.S, tss.UI.VStack(), 50)), 8), [tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), 4), tss.ICX.PT(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), 4), tss.UI.Button$1("Click Me").Primary(), tss.UI.Label$1("Icon:").Inline().SetContent(tss.UI.Icon$2(2369, "fi-br-", "tss-fontsize-large", void 0)), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50)), 4)])]);
                                }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                            return $tcs.task;
                        });
                    })(this, delay), tss.ICTX.Children$6(tss.S, tss.ICX.HS(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack())), [tss.ICX.H(tss.Image, tss.ICX.W(tss.Image, tss.UI.Image$1(""), 40), 40), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.ICX.W(tss.S, tss.UI.VStack(), 50)), 8), [tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), 4), tss.ICX.PT(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), 4), tss.UI.Button$1("Click Me").Primary(), tss.UI.Label$1("Icon:").Inline().SetContent(tss.UI.Icon$2(2369, "fi-br-", "tss-fontsize-large", void 0)), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50)), 4)])]).Skeleton())]));
                }
            },
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DeferWithProgressSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var container = tss.UI.VStack();
                var trigger = new (tss.SettableObservableT(System.Int32))(0);

                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.DeferWithProgressSample, 1110, "A utility to defer execution with progress"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("DeferWithProgress extends Defer by providing a way to report progress during the async operation. This is useful for long-running tasks where you want to show a progress bar or status updates.")])).SetTitle("Overview"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Click the button below to start a simulated long-running task with progress reporting."), tss.UI.Button$1("Start Task").Primary().OnClick$1(function () {
                    container.Clear();
                    container.Add(tss.UI.DeferWithProgress(function (reportProgress) {
                        var $tcs = new H5.TCS();
                        (async () => {
                            {
                                for (var i = 0; i <= 100; i = (i + 10) | 0) {
                                    reportProgress(i / 100.0, System.String.format("Processing step {0} of 10...", [H5.box(((H5.Int.div(i, 10)) | 0), System.Int32)]));
                                    (await H5.toPromise(System.Threading.Tasks.Task.delay(500)));
                                }
                                return tss.ICX.P(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.txtX.Success(tss.txt, tss.UI.TextBlock("Task Completed Successfully!"))), 10);
                            }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                        return $tcs.task;
                    }));
                }), container])).SetTitle("Basic Usage")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("DeferWithProgress can also observe values and refresh when they change, passing the observed values to the async generator."), tss.UI.Button$1("Trigger Refresh").Primary().OnClick$1(function () {
                    var $t;
                    H5.identity(trigger.Value$1, (($t = (trigger.Value$1 + 1) | 0, trigger.Value$1 = $t, $t)));
                }), tss.UI.DeferWithProgress$1(System.Int32, trigger, function (val, progress) {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            progress(0, "Starting...");
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(500)));
                            progress(0.3, "Step 1 complete");
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(500)));
                            progress(0.6, "Step 2 complete");
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(500)));
                            progress(0.9, "Finalizing...");
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(500)));
                            return tss.txtX.Success(tss.txt, tss.UI.TextBlock(System.String.format("Loaded content for trigger value: {0}", [H5.box(val, System.Int32)])));
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                })])).SetTitle("Usage with Observables")]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DeltaComponentSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var deltaContainer = document.createElement("div");
                var deltaComponent = tss.UI.DeltaComponent(tss.UI.Raw$2(deltaContainer)).Animated();

                var html = "";
                var step = 1;

                var typing = tss.UI.Button$1("Type Lorem Ipsum").OnClick$1(function () {
                    var TypeNextChar = null;
                    var lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris  nisi ut aliquip ex ea commodo consequat.";

                    var d1 = document.createElement("div");
                    d1.innerHTML = "<div><span></span><b>Starting...</b></div>";
                    deltaComponent.ReplaceContent(tss.UI.Raw$2(d1));

                    var index = 0;


                    TypeNextChar = function () {
                        if (index > lorem.length) {
                            var dFinal = document.createElement("div");
                            dFinal.innerHTML = System.String.format("<div><span>{0}</span><b> Done \u2714</b></div>", [lorem]);
                            deltaComponent.ReplaceContent(tss.UI.Raw$2(dFinal));
                            return;
                        }

                        var currentText = lorem.substr(0, index);
                        var d = document.createElement("div");
                        d.innerHTML = System.String.format("<div><span>{0}</span><b>Typing... {1}/{2}</b></div>", currentText, H5.box(index, System.Int32), H5.box(lorem.length, System.Int32));
                        deltaComponent.ReplaceContent(tss.UI.Raw$2(d));
                        index = (index + 1) | 0;
                        window.setTimeout(function (_) {
                            TypeNextChar();
                        }, 25);
                    };

                    TypeNextChar();
                });

                var typingWithComponents = tss.UI.Button$1("Type Lorem Ipsum 2").OnClick$1(function () {
                    var $t;
                    var TypeNextChar = null;
                    var lorem = ($t = System.Char, System.Linq.Enumerable.from("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris  nisi ut aliquip ex ea commodo consequat.", $t).ToArray($t));

                    var stack = tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), [tss.UI.TextBlock("Starting...")]);

                    deltaComponent.ReplaceContent(stack);

                    var index = 0;


                    TypeNextChar = function () {
                        if (index > lorem.length) {
                            stack = tss.ICTX.Children$1(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), System.Linq.Enumerable.from(lorem, System.Char).select(function (t) {
                                    return tss.ICX.PR(tss.txt, tss.UI.TextBlock(String.fromCharCode(t)), t === 32 ? 4 : 0);
                                }).ToArray(tss.txt), tss.ICX.PR(tss.Icon, tss.UI.Icon$2(964), 8));
                            deltaComponent.ReplaceContent(stack);
                            return;
                        }

                        var currentText = System.Linq.Enumerable.from(lorem, System.Char).take(index).ToArray(System.Char);
                        stack = tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), System.Linq.Enumerable.from(currentText, System.Char).select(function (t) {
                                return tss.ICX.PR(tss.txt, tss.UI.TextBlock(String.fromCharCode(t)), t === 32 ? 4 : 0);
                            }).ToArray(tss.txt));
                        deltaComponent.ReplaceContent(stack);
                        index = (index + 1) | 0;
                        window.setTimeout(function (_) {
                            TypeNextChar();
                        }, 25);
                    };

                    TypeNextChar();
                });

                var resetBtn = tss.UI.Button$1("Reset").OnClick$1(function () {
                    html = "";
                    step = 1;
                    var d = document.createElement("div");
                    deltaComponent.ReplaceContent(tss.UI.Raw$2(d));
                });

                var shadowContainer = document.createElement("div");
                var shadowDeltaComponent = tss.UI.DeltaComponent(tss.UI.Raw$2(shadowContainer), true).Animated();

                var shadowTyping = tss.UI.Button$1("Type in Shadow DOM").OnClick$1(function () {
                    var TypeNextChar = null;
                    var lorem = "This text is inside a Shadow DOM!";

                    var d1 = document.createElement("div");
                    d1.innerHTML = "<div><span></span><b>Shadow Starting...</b></div>";
                    shadowDeltaComponent.ReplaceContent(tss.UI.Raw$2(d1));

                    var index = 0;


                    TypeNextChar = function () {
                        if (index > lorem.length) {
                            var dFinal = document.createElement("div");
                            dFinal.innerHTML = System.String.format("<div><span>{0}</span><b> Shadow Done \u2714</b></div>", [lorem]);
                            shadowDeltaComponent.ReplaceContent(tss.UI.Raw$2(dFinal));
                            return;
                        }

                        var currentText = lorem.substr(0, index);
                        var d = document.createElement("div");
                        d.innerHTML = System.String.format("<div><span>{0}</span><b>Shadow Typing... {1}/{2}</b></div>", currentText, H5.box(index, System.Int32), H5.box(lorem.length, System.Int32));
                        shadowDeltaComponent.ReplaceContent(tss.UI.Raw$2(d));
                        index = (index + 1) | 0;
                        window.setTimeout(function (_) {
                            TypeNextChar();
                        }, 25);
                    };

                    TypeNextChar();
                });

                var shadowResetBtn = tss.UI.Button$1("Reset Shadow").OnClick$1(function () {
                    var d = document.createElement("div");
                    d.textContent = "Shadow DOM Initial Content";
                    shadowDeltaComponent.ReplaceContent(tss.UI.Raw$2(d));
                });


                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), tss.DeltaComponent, 3728, "A component that animates changes"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("DeltaComponent updates its DOM tree to match a new component's DOM tree using a diff algorithm. It detects text appends and adds them as new spans to avoid full re-rendering."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [typing, typingWithComponents, resetBtn])])).SetTitle("Overview"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [deltaComponent])).SetTitle("Output"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("This DeltaComponent renders its content inside a Shadow DOM root."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [shadowTyping, shadowResetBtn]), shadowDeltaComponent])).SetTitle("Shadow DOM")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DetailsListSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var query = tss.Router.GetQueryParameters();
                var queryPageStr = { };
                var queryPage = { };
                var page = query.ContainsKey("page") && query.TryGetValue("page", queryPageStr) && System.Int32.tryParse(queryPageStr.v, queryPage) ? queryPage.v : 2;

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.DetailsListSample, 2773, "A list that displays details"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("DetailsList is a robust way to display an information-rich collection of items. It supports sorting, grouping, filtering, and pagination."), tss.UI.TextBlock("It is classically used for file explorers, database record views, or any scenario where information density is critical.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use DetailsList when users need to compare items across multiple metadata fields. Display columns in order of importance from left to right. Provide ample default width for each column to avoid unnecessary truncation. Use compact mode when vertical space is limited or when displaying very large datasets. Always provide a clear empty state message if the list contains no items.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standard File List"), tss.UI.TextBlock("A list with textual rows, supporting sorting and custom column widths."), tss.ICX.MB(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1864), tss.usX.px$1(32), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.px$1(350), true, true, "FileName", void 0), tss.UI.DetailsListColumn("Date Modified", tss.usX.px$1(170), false, true, "DateModified", void 0), tss.UI.DetailsListColumn("Modified By", tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn("File Size", tss.usX.px$1(120), false, true, "FileSize", void 0)]), tss.usX.px$1(400)).WithListItems(this.GetDetailsListItems()).SortedBy("FileName"), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Responsive Widths"), tss.UI.TextBlock("Using percentage-based widths with max-width constraints."), tss.ICX.MB(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.WidthStretch(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1864), tss.usX.px$1(64), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.percent$1(40), true, true, "FileName", void 0), tss.UI.DetailsListColumn$1("Date Modified", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "DateModified", void 0), tss.UI.DetailsListColumn$1("Modified By", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn$1("File Size", tss.usX.percent$1(10), tss.usX.px$1(150), false, true, "FileSize", void 0)]), tss.usX.px$1(400))).WithListItems(this.GetDetailsListItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Components in Rows"), tss.UI.TextBlock("DetailsList can host any Tesserae component within its cells."), tss.ICX.MB(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents, [tss.UI.IconColumn(tss.UI.Icon$2(150), tss.usX.px$1(32), false, void 0, void 0), tss.UI.DetailsListColumn("Status", tss.usX.px$1(120), false, false, void 0, void 0), tss.UI.DetailsListColumn("Name", tss.usX.px$1(250), true, false, void 0, void 0), tss.UI.DetailsListColumn("Action", tss.usX.px$1(150), false, false, void 0, void 0), tss.UI.DetailsListColumn("Rating", tss.usX.px$1(400), false, false, void 0, void 0)]).Compact(), tss.usX.px$1(400)).WithListItems(this.GetComponentDetailsListItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Paginated and Empty States"), tss.UI.SplitView().Resizable().SplitInMiddle().Left(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Infinite scrolling")), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1864), tss.usX.px$1(64), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.percent$1(40), true, true, "FileName", void 0), tss.UI.DetailsListColumn$1("Date Modified", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "DateModified", void 0), tss.UI.DetailsListColumn$1("Modified By", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn$1("File Size", tss.usX.percent$1(10), tss.usX.px$1(150), false, true, "FileSize", void 0)]), tss.usX.px$1(300)).WithListItems(this.GetDetailsListItems(0, page)).WithPaginatedItems(H5.fn.bind(this, function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            page = (page + 1) | 0;
                            tss.Router.ReplaceQueryParameters(function (p) {
                                return p.With("page", H5.toString(page));
                            });
                            return (await H5.toPromise(this.GetDetailsListItemsAsync(page, 5)));
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }))])).Right(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Empty State")), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1864), tss.usX.px$1(64), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.percent$1(40), true, true, "FileName", void 0), tss.UI.DetailsListColumn$1("Date Modified", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "DateModified", void 0), tss.UI.DetailsListColumn$1("Modified By", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn$1("File Size", tss.usX.percent$1(10), tss.usX.px$1(150), false, true, "FileSize", void 0)]).WithEmptyMessage(function () {
                    return tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No items found"), tss.usX.px$1(16))))));
                }), tss.usX.px$1(300)).WithListItems(System.Array.init(0, null, Tesserae.Tests.Samples.DetailsListSampleFileItem))]))])).SetTitle("Usage")]));
            }
        },
        methods: {
            GetDetailsListItemsAsync: function (start, count) {
                var $tcs = new H5.TCS();
                (async () => {
                    {
                        if (start === void 0) { start = 1; }
                        if (count === void 0) { count = 100; }
                        (await H5.toPromise(System.Threading.Tasks.Task.delay(1000)));
                        return this.GetDetailsListItems(start, count);
                    }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                return $tcs.task;
            },
            GetDetailsListItems: function (start, count) {
                if (start === void 0) { start = 1; }
                if (count === void 0) { count = 100; }
                return System.Linq.Enumerable.range(start, count).select(function (n) {
                    return new Tesserae.Tests.Samples.DetailsListSampleFileItem(1903, System.String.format("Document_{0}.docx", [H5.box(n, System.Int32)]), System.DateTime.addDays(System.DateTime.getToday(), ((-n) | 0)), "System", n * 1.5);
                }).ToArray(Tesserae.Tests.Samples.DetailsListSampleFileItem);
            },
            GetComponentDetailsListItems: function () {
                return System.Linq.Enumerable.range(1, 20).select(function (n) {
                    return new Tesserae.Tests.Samples.DetailsListSampleItemWithComponents().WithIcon(1424).WithCheckBox(tss.UI.CheckBox$1("Active").Checked()).WithName(System.String.format("Record {0}", [H5.box(n, System.Int32)])).WithButton(tss.UI.Button$1("Edit").Primary().OnClick(function (s, e) {
                        tss.UI.Toast().Information(System.String.format("Editing {0}", [H5.box(n, System.Int32)]));
                    })).WithChoiceGroup(tss.UI.ChoiceGroup().Horizontal().Choices([tss.UI.Choice("A"), tss.UI.Choice("B"), tss.UI.Choice("C")]));
                }).ToArray(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents);
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DialogSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var dialog = tss.UI.Dialog("Sample Dialog");
                var response = tss.UI.TextBlock$1();

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.DialogSample, 1222, "A dialog component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Dialogs are modal UI overlays that provide contextual information or require user action, such as confirmation or input. They are designed to capture the user's attention and typically block interaction with the rest of the application until they are dismissed.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Dialogs for critical or short-term tasks that require a decision. Ensure the content is brief and clearly states the purpose. Provide logical action buttons (e.g., 'Confirm' and 'Cancel') and highlight the primary action. Avoid overusing Dialogs for non-essential information to prevent frustrating the user. Consider using non-modal alternatives if the task doesn't require immediate attention.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Button$1("Open Dialog").OnClick(function (c, ev) {
                    dialog.Show();
                }), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1("Open YesNo").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").YesNo(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Yes");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked No");
                    });
                }), tss.UI.Button$1("Open YesNoCancel").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").YesNoCancel(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Yes");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked No");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Cancel");
                    });
                }), tss.UI.Button$1("Open Ok").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").Ok(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Ok");
                    });
                }), tss.UI.Button$1("Open RetryCancel").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").RetryCancel(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Retry");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Cancel");
                    });
                })]), tss.UI.Button$1("Open YesNo with dark overlay").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").Dark().YesNo(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Yes");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked No");
                    }, function (y) {
                        return y.Success().SetText("Yes!");
                    }, function (n) {
                        return n.Danger().SetText("Nope");
                    });
                }), tss.UI.Button$1("Open YesNoCancel with dark overlay").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").Dark().YesNoCancel(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Yes");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked No");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Cancel");
                    });
                }), tss.UI.Button$1("Open Ok with dark overlay").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").Dark().Ok(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Ok");
                    });
                }), tss.UI.Button$1("Open RetryCancel with dark overlay").OnClick(function (c, ev) {
                    tss.UI.Dialog("Sample Dialog").Dark().RetryCancel(function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Retry");
                    }, function () {
                        tss.txtX.Text(tss.txt, response, "Clicked Cancel");
                    });
                }), response])).SetTitle("Usage")]));

                dialog.Content(tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit."), tss.UI.Toggle$1("Is draggable").OnChange(function (c, ev) {
                    dialog.IsDraggable = c.IsChecked;
                }), tss.UI.Toggle$1("Is dark overlay").OnChange(function (c, ev) {
                    dialog.IsDark = c.IsChecked;
                }).Checked(dialog.IsDark)])).Commands([tss.ICX.AlignEnd(tss.Button, tss.UI.Button$1("Send").Primary()).OnClick(function (c, ev) {
                    dialog.Hide();
                }), tss.ICX.AlignEnd(tss.Button, tss.UI.Button$1("Don`t send")).OnClick(function (c, ev) {
                    dialog.Hide();
                })]);
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.DropdownSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                StartLoadingAsyncDataImmediately: function (dropdown) {
                    tss.tX.fireAndForget(dropdown.LoadItemsAsync());
                    return dropdown;
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var validatedDropdown = tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1("Option 1"), tss.UI.DropdownItem$1("Option 2")]);
                validatedDropdown.Attach(function (dd) {
                    var $t;
                    dd.IsInvalid = dd.SelectedItems.length !== 1 || !H5.rE(($t = dd.SelectedItems)[System.Array.index(0, $t)].Text, "Option 1");
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.DropdownSample, 869, "A control to select an option from a dropdown"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A Dropdown is a list in which the selected item is always visible, and the others are visible on demand by clicking a drop-down button."), tss.UI.TextBlock("They are used to simplify the design and make a choice within the UI. When closed, only the selected item is visible. When users click the drop-down button, all the options become visible.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use a Dropdown when there are multiple choices that can be collapsed under one title, especially if the list of items is long or when space is constrained. Use shortened statements or single words as options. Dropdowns are preferred over radio buttons when the selected option is more important than the alternatives. For less than 7 options, consider using a ChoiceGroup if space allows.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Dropdown"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1("Option 1").Selected(), tss.UI.DropdownItem$1("Option 2"), tss.UI.DropdownItem$1("Option 3")])), tss.UI.Label$1("With Headers and Dividers").SetContent(tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1("Group 1").Header(), tss.UI.DropdownItem$1("Item 1.1"), tss.UI.DropdownItem$1("Item 1.2"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Group 2").Header(), tss.UI.DropdownItem$1("Item 2.1"), tss.UI.DropdownItem$1("Item 2.2").Selected()]))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Selection Modes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Searchable").SetContent(tss.UI.Dropdown().Searchable().Items$1([tss.UI.DropdownItem$1("Apple"), tss.UI.DropdownItem$1("Banana").Selected(), tss.UI.DropdownItem$1("Orange").Selected(), tss.UI.DropdownItem$1("Grape")])), tss.UI.Label$1("Multi-select").SetContent(tss.UI.Dropdown().Multi().Items$1([tss.UI.DropdownItem$1("Apple"), tss.UI.DropdownItem$1("Banana").Selected(), tss.UI.DropdownItem$1("Orange").Selected(), tss.UI.DropdownItem$1("Grape")])), tss.UI.Label$1("Custom Arrow Icon").SetContent(tss.UI.Dropdown().SetArrowIcon(130).Items$1([tss.UI.DropdownItem$1("Low"), tss.UI.DropdownItem$1("Medium").Selected(), tss.UI.DropdownItem$1("High")]))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Async Loading"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Load on open (5s delay)").SetContent(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetItemsAsync))), tss.UI.Label$1("Load immediately (5s delay)").SetContent(Tesserae.Tests.Samples.DropdownSample.StartLoadingAsyncDataImmediately(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetItemsAsync)))), tss.UI.Label$1("Deferred item content").SetContent(tss.UI.Dropdown().Items$1([this.DeferredDropdownItem("Item 1", 500), this.DeferredDropdownItem("Item 2", 800), this.DeferredDropdownItem("Item 3", 1100)])), tss.UI.Label$1("Empty State").SetContent(tss.UI.Dropdown$2("No items available").Items$1(System.Array.init(0, null, tss.Dropdown.Item)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Required Dropdown").SetContent(tss.UI.Dropdown().Required().Items$1([tss.UI.DropdownItem$1("Choose one...").Header(), tss.UI.DropdownItem$1("Valid Choice")])), tss.UI.Label$1("Validation (Must select 'Option 1')").SetContent(validatedDropdown)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Dropdowns"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Small").SetContent(tss.ISX.Rounded(tss.Dropdown, tss.UI.Dropdown(), tss.BR.Small).Items$1([tss.UI.DropdownItem$1("Option 1"), tss.UI.DropdownItem$1("Option 2")])), tss.UI.Label$1("Medium").SetContent(tss.ISX.Rounded(tss.Dropdown, tss.UI.Dropdown(), tss.BR.Medium).Items$1([tss.UI.DropdownItem$1("Option 1"), tss.UI.DropdownItem$1("Option 2")])), tss.UI.Label$1("Full").SetContent(tss.ISX.Rounded(tss.Dropdown, tss.UI.Dropdown(), tss.BR.Full).Items$1([tss.UI.DropdownItem$1("Option 1"), tss.UI.DropdownItem$1("Option 2")]))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            DeferredDropdownItem: function (text, delayMs) {
                return tss.UI.DropdownItem$2(tss.UI.Defer$1(H5.fn.bind(this, function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(delayMs)));
                            return tss.UI.Label$1(System.String.format("Loaded {0}", [text]));
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }), tss.ICX.H(tss.Skeleton, tss.ICX.W(tss.Skeleton, tss.UI.Skeleton().Animated(), 500), 32)));
            },
            GetItemsAsync: function () {
                var $tcs = new H5.TCS();
                (async () => {
                    {
                        (await H5.toPromise(System.Threading.Tasks.Task.delay(5000)));
                        return System.Array.init([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("Async Item 1"), tss.UI.DropdownItem$1("Async Item 2"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("Async Item 3")], tss.Dropdown.Item);
                    }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                return $tcs.task;
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.EditableLabelSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.EditableLabelSample, 1663, "A label that can be edited"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("EditableLabels and EditableAreas allow users to view content as standard text and switch to an editing mode (input or textarea) upon interaction."), tss.UI.TextBlock("They are useful for 'in-place' editing where you want to keep the UI clean but allow users to quickly modify specific fields without navigating to a separate form.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use EditableLabels for short, single-line content like titles or names. Use EditableAreas for longer, multi-line content like descriptions. Always provide an OnSave() callback to persist the changes. Ensure the interaction to trigger editing is clear\u2014typically by showing an edit icon on hover or using a distinct visual style. Consider using validation to ensure the entered data meets your requirements.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Editable Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.EditableLabel("Click to edit this text"), tss.ITFX.Bold(tss.EditableLabel, tss.ITFX.Large(tss.EditableLabel, tss.UI.EditableLabel("Large and Bold Title"))), tss.ITFX.MediumPlus(tss.EditableLabel, tss.UI.EditableLabel("Pre-configured font size"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Editable Area"), tss.UI.TextBlock("For multi-line text input:"), tss.ICX.Width(tss.EditableArea, tss.UI.EditableArea("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Click here to edit the entire block of text."), tss.usX.px$1(400)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Events and Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.EditableLabel("Change me and check the toast").OnSave(function (s, text) {
                    tss.UI.Toast().Success(System.String.format("Saved: {0}", [text]));
                    return true;
                }), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required Field")).SetContent(tss.UI.EditableLabel("Can't be empty"))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.EmojiSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                ToValidName: function (icon) {
                    var words = System.Linq.Enumerable.from(System.String.split(icon, System.Array.init([45], System.Char).map(function (i) {{ return String.fromCharCode(i); }}), null, 1), System.String).select(function (i) {
                            return (i.substr(0, 1).toUpperCase() || "") + (i.substr(1) || "");
                        }).ToArray(System.String);

                    var name = (words).join("");

                    if (System.Char.isDigit(name.charCodeAt(0))) {
                        return "_" + (name || "");
                    } else {
                        return name;
                    }
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                var $t;
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.EmojiSample, 4133, "A utility to display emojis"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Tesserae includes full support for Emojis via an integrated stylesheet and a strongly-typed enum. This allows you to easily add expressive icons to your application with complete C# IntelliSense support and consistent rendering.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Emojis to add personality and visual cues to your interface. Ensure that the Emojis used are universally understood and appropriate for the context. Avoid using Emojis as the sole way to convey critical information, as their appearance can vary slightly between platforms. Use the SearchableList below to find the exact Emoji you need.")])).SetTitle("Best Practices")])), tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle(System.String.format("Strongly-typed {0} enum", ["Emoji"])), tss.UI.SearchableList(Tesserae.Tests.Samples.EmojiSample.IconItem, ($t = Tesserae.Tests.Samples.EmojiSample.IconItem, System.Linq.Enumerable.from(this.GetAllIcons(), $t).ToArray($t)), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)])])).SetTitle("Usage")])), true, false, "");
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetAllIcons: function () {
                return new (H5.GeneratorEnumerable$1(Tesserae.Tests.Samples.EmojiSample.IconItem))(H5.fn.bind(this, function ()  {
                    var $s = 0,
                        $jff,
                        $rv,
                        names,
                        values,
                        i,
                        $ae;

                    var $en = new (H5.GeneratorEnumerator$1(Tesserae.Tests.Samples.EmojiSample.IconItem))(H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                switch ($s) {
                                    case 0: {
                                        names = System.Enum.getNames(Tesserae.Emoji);
                                            values = H5.cast(System.Enum.getValues(Tesserae.Emoji), System.Array.type(Tesserae.Emoji));

                                            i = 0;
                                            $s = 1;
                                            continue;
                                    }
                                    case 1: {
                                        if ( i < values.length ) {
                                                $s = 2;
                                                continue;
                                            }
                                        $s = 5;
                                        continue;
                                    }
                                    case 2: {
                                        $en.current = new Tesserae.Tests.Samples.EmojiSample.IconItem(values[System.Array.index(i, values)], names[System.Array.index(i, names)]);
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $s = 4;
                                        continue;
                                    }
                                    case 4: {
                                        i = (i + 1) | 0;
                                        $s = 1;
                                        continue;
                                    }
                                    case 5: {

                                    }
                                    default: {
                                        return false;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            throw $ae;
                        }
                    }));
                    return $en;
                }));
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.FileSelectorAndDropAreaSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var size = { };
                var droppedFiles = { };
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.FileSelectorAndDropAreaSample, 1900, "A control to select and drop files"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("FileSelector and FileDropArea provide two different ways for users to upload files. FileSelector uses a standard button that opens the system file dialog, while FileDropArea provides a larger target area for users to drag and drop files directly into the application.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use FileSelector for simple, single-file selections in forms. Use FileDropArea when users are likely to be uploading multiple files or when a more prominent upload target is desired. Always specify the allowed file types using the 'Accepts' property. Provide immediate feedback after files are selected or dropped, such as displaying the file names or sizes.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("File Selector"), tss.UI.Label$1("Selected file size: ").Inline().SetContent(tss.UI.Var(tss.txt, tss.UI.TextBlock(""), size)), tss.UI.FileSelector().OnFileSelected(function (fs, e) {
                    size.v.Text = (H5.toString(fs.SelectedFile.size) || "") + " bytes";
                }), tss.UI.FileSelector().SetPlaceholder("You must select a zip file").Required().SetAccepts(".zip").OnFileSelected(function (fs, e) {
                    size.v.Text = (H5.toString(fs.SelectedFile.size) || "") + " bytes";
                }), tss.UI.FileSelector().SetPlaceholder("Please select any image").SetAccepts("image/*").OnFileSelected(function (fs, e) {
                    size.v.Text = (H5.toString(fs.SelectedFile.size) || "") + " bytes";
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("File Drop Area"), tss.UI.Label$1("Dropped Files: ").SetContent(tss.UI.Var(tss.S, tss.UI.Stack(), droppedFiles)), tss.UI.FileDropArea().OnFilesDropped(function (s, e) {
                    var $t;
                    $t = H5.getEnumerator(e);
                    try {
                        while ($t.moveNext()) {
                            var file = $t.Current;
                            droppedFiles.v.Add(tss.ITFX.Small(tss.txt, tss.UI.TextBlock(file.name)));
                        }
                    } finally {
                        if (H5.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                }).Multiple()])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.FloatSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.FloatSample, 151, "A component to display floating content"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Float components are used to place content in absolute-positioned overlays within a relative container. They allow for precise placement of UI elements, such as badges, help icons, or status indicators, without affecting the layout of surrounding components.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Float when you need to position an element independently of the normal document flow. Always ensure the parent container is set to 'Relative' positioning to constrain the floated element. Be careful not to obscure important content or interactive elements beneath the overlay. Use meaningful positions that correlate logically with the parent content.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Possible Positions")), tss.ICTX.Children$6(tss.S, tss.ICX.Height(tss.S, tss.ICX.WS(tss.S, tss.UI.Stack().Relative().Horizontal()), tss.usX.px$1(400)), [tss.UI.Float(tss.UI.Button$1("TopLeft"), "tss-float-topleft"), tss.UI.Float(tss.UI.Button$1("TopMiddle"), "tss-float-topmiddle"), tss.UI.Float(tss.UI.Button$1("TopRight"), "tss-float-topright"), tss.UI.Float(tss.UI.Button$1("LeftCenter"), "tss-float-leftcenter"), tss.UI.Float(tss.UI.Button$1("Center"), "tss-float-center"), tss.UI.Float(tss.UI.Button$1("RightCenter"), "tss-float-rightcenter"), tss.UI.Float(tss.UI.Button$1("BottomLeft"), "tss-float-bottomleft"), tss.UI.Float(tss.UI.Button$1("BottonMiddle"), "tss-float-bottonmiddle"), tss.UI.Float(tss.UI.Button$1("BottomRight"), "tss-float-bottomright")])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.GradientsSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var Render = null;
                var allGradients = System.Array.init([new (System.ValueTuple$2(System.String,System.String)).$ctor1("Lime", tss.UI.Theme.Gradients.Lime), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Red", tss.UI.Theme.Gradients.Red), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Orange", tss.UI.Theme.Gradients.Orange), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Yellow", tss.UI.Theme.Gradients.Yellow), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Green", tss.UI.Theme.Gradients.Green), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Teal", tss.UI.Theme.Gradients.Teal), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Blue", tss.UI.Theme.Gradients.Blue), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Purple", tss.UI.Theme.Gradients.Purple), new (System.ValueTuple$2(System.String,System.String)).$ctor1("Magenta", tss.UI.Theme.Gradients.Magenta), new (System.ValueTuple$2(System.String,System.String)).$ctor1("AI", tss.UI.Theme.Gradients.AI)], System.ValueTuple$2(System.String,System.String));

                var grid = tss.UI.Grid([tss.usX.fr$1(1), tss.usX.fr$1(1), tss.usX.fr$1(1)]).Gap(tss.usX.px$1(8));


                Render = H5.fn.bind(this, function () {
                    tss.ICTX.Children$6(tss.Grid, grid, [tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.UI.VStack(), System.Linq.Enumerable.from(allGradients, System.ValueTuple$2(System.String,System.String)).select(H5.fn.bind(this, function (g) {
                            return this.RenderGradientStack(g.Item1, g.Item2);
                        })).ToArray(tss.IC))])).SetTitle("Linear Gradients")])]);
                });
                Render();

                tss.UI.Theme.addOnThemeChanged(function () {
                    window.setTimeout(function (_) {
                        Render();
                    }, 1);
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.GradientsSample, 3262, "A utility to apply gradients"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Tesserae provides a comprehensive set of predefined gradients that are part of the theme. These gradients are accessible via the 'Theme.Gradients' class and are designed to provide a consistent visual language across the application, with support for both light and dark modes.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Prefer using these predefined gradients over hardcoded linear-gradient functions to ensure your application remains consistent with the theme. Click on any gradient name below to copy its C# constant name.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [grid])).SetTitle("Usage")]));
            }
        },
        methods: {
            RenderGradientStack: function (gradientName, gradientVar) {
                var textColor = "white";
                return tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.UI.HStack().NoWrap(), gradientVar), [tss.ICX.Grow(tss.Button, tss.ICX.W(tss.Button, tss.ISX.Foreground(tss.Button, tss.UI.Button$1(gradientName), textColor).NoBackground(), 10)).OnClick$1(function () {
                    tss.Clipboard.Copy(System.String.format("Theme.Gradients.{0}", [gradientName]));
                })])]), 8);
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.GridPickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                GetGameOfLifeSample: function () {
                    var Grow = null;
                    var GetNeighbors = null;
                    var height = 32;
                    var width = 32;
                    var isPaused = false;

                    var grid = tss.UI.GridPicker(System.Linq.Enumerable.range(0, width).select(function (n) {
                        return System.String.format("{0:00}", [H5.box(n, System.Int32)]);
                    }).ToArray(System.String), System.Linq.Enumerable.range(0, height).select(function (n) {
                        return System.String.format("{0:00}", [H5.box(n, System.Int32)]);
                    }).ToArray(System.String), 2, System.Linq.Enumerable.range(0, width).select(function (_) {
                        return System.Array.init(height, 0, System.Int32);
                    }).ToArray(System.Array.type(System.Int32)), function (btn, state, previousState) {
                        var color = state === 0 ? tss.UI.Theme.Default.Background : tss.UI.Theme.Default.Foreground;
                        tss.ISX.Background(tss.Button, btn, color);
                    }, System.Array.init([tss.usX.px$1(128), tss.usX.px$1(24)], tss.us), tss.usX.px$1(24));
                    Grow = function () {
                        var $t, $t1, $t2;
                        if (grid.IsDragging || isPaused) {
                            return;
                        }
                        var previous = grid.GetState();
                        var cells = grid.GetState();
                        for (var i = 0; i < height; i = (i + 1) | 0) {
                            for (var j = 0; j < width; j = (j + 1) | 0) {
                                var alive = GetNeighbors(previous, i, j);
                                if (($t = cells[System.Array.index(i, cells)])[System.Array.index(j, $t)] === 1) {
                                    ($t1 = cells[System.Array.index(i, cells)])[System.Array.index(j, $t1)] = (alive < 2 || alive > 3) ? 0 : 1;
                                } else {
                                    if (alive === 3) {
                                        ($t2 = cells[System.Array.index(i, cells)])[System.Array.index(j, $t2)] = 1;
                                    }
                                }
                            }
                        }

                        grid.SetState(cells);
                    };
                    GetNeighbors = function (cells, x, y) {
                        var $t;
                        var count = 0;
                        for (var i = (x - 1) | 0; i <= ((x + 1) | 0); i = (i + 1) | 0) {
                            for (var j = (y - 1) | 0; j <= ((y + 1) | 0); j = (j + 1) | 0) {
                                if (i < 0 || j < 0 || i >= height || j >= width || (i === x && j === y)) {
                                    continue;
                                }
                                if (($t = cells[System.Array.index(i, cells)])[System.Array.index(j, $t)] === 1) {
                                    count = (count + 1) | 0;
                                }
                            }
                        }

                        return count;
                    };
                    tss.ICX.WhenMounted(tss.GridPicker, grid, function () {
                        var t = window.setInterval(function (_) {
                            if (tss.UI.IsMounted$1(grid)) {
                                Grow();
                            }
                        }, 200);
                        tss.ICX.WhenRemoved(tss.GridPicker, grid, function () {
                            window.clearInterval(t);
                        });
                    });

                    var btnReset = tss.UI.Button$1("Reset").SetIcon$1(499).OnClick$1(function () {
                        var $t;
                        var state = grid.GetState();
                        $t = H5.getEnumerator(state);
                        try {
                            while ($t.moveNext()) {
                                var a = $t.Current;
                                for (var i = 0; i < a.length; i = (i + 1) | 0) {
                                    a[System.Array.index(i, a)] = 0;
                                }
                            }
                        } finally {
                            if (H5.is($t, System.IDisposable)) {
                                $t.System$IDisposable$Dispose();
                            }
                        }
                        grid.SetState(state);
                    });

                    var btnPause = tss.UI.Button$1("Pause").SetIcon$1(3300);
                    btnPause.OnClick$1(function () {
                        isPaused = !isPaused;
                        btnPause.SetIcon$1(isPaused ? 3515 : 3300);
                        btnPause.SetText(isPaused ? "Resume" : "Pause");
                    });





                    return tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), [btnPause, btnReset]), tss.ICX.WS(tss.GridPicker, grid)]);
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var picker = tss.UI.GridPicker(System.Array.init(["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"], System.String), System.Array.init(["Morning", "Afternoon", "Night"], System.String), 3, System.Array.init([System.Array.init([0, 0, 0, 0, 0, 0, 0], System.Int32), System.Array.init([0, 0, 0, 0, 0, 0, 0], System.Int32), System.Array.init([0, 0, 0, 0, 0, 0, 0], System.Int32)], System.Array.type(System.Int32)), function (btn, state, previousState) {
                    var text = "";
                    switch (state) {
                        case 0: 
                            text = "\u2620";
                            break;
                        case 1: 
                            text = "\ud83d\udc22";
                            break;
                        case 2: 
                            text = "\ud83d\udc07";
                            break;
                    }

                    if (previousState >= 0 && previousState !== state) {
                        switch (previousState) {
                            case 0: 
                                text = System.String.format("\u2620 -> {0}", [text]);
                                break;
                            case 1: 
                                text = System.String.format("\ud83d\udc22 -> {0}", [text]);
                                break;
                            case 2: 
                                text = System.String.format("\ud83d\udc07 -> {0}", [text]);
                                break;
                        }
                    }
                    btn.SetText(text);
                }, void 0, void 0);

                var hourPicker = tss.UI.GridPicker(System.Linq.Enumerable.range(0, 24).select(function (n) {
                    return System.String.format("{0:00}", [H5.box(n, System.Int32)]);
                }).ToArray(System.String), System.Array.init(["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"], System.String), 4, System.Linq.Enumerable.range(0, 7).select(function (_) {
                    return System.Array.init(24, 0, System.Int32);
                }).ToArray(System.Array.type(System.Int32)), function (btn, state, previousState) {
                    var color = "";
                    switch (state) {
                        case 0: 
                            color = "#c7c5c5";
                            break;
                        case 1: 
                            color = "#a3cfa5";
                            break;
                        case 2: 
                            color = "#76cc79";
                            break;
                        case 3: 
                            color = "#1fcc24";
                            break;
                    }
                    tss.ISX.Background(tss.Button, btn, color);
                }, System.Array.init([tss.usX.px$1(128), tss.usX.px$1(24)], tss.us), tss.usX.px$1(24));

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.GridPickerSample, 4439, "A control to pick an item from a grid"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The GridPicker component provides an interactive grid where each cell can cycle through a predefined number of states. It's highly customizable through its state formatting logic."), tss.UI.TextBlock("Common use cases include scheduling, availability maps, or any scenario where you need to visualize and edit state across two dimensions.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use GridPickers for dense state selection where labels for each cell would be too cluttered. Provide a clear legend or visual cues for what each state represents. Ensure the row and column headers are descriptive. If the grid is large, consider how it will behave on smaller screens. Leverage the state formatting to provide rich feedback, such as changing colors, icons, or text based on the current state.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Simple Schedule Example"), tss.UI.TextBlock("Click on cells to cycle through states: Dead (\u2620), Slow (\ud83d\udc22), and Fast (\ud83d\udc07)."), picker, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Heatmap/Availability Example"), tss.UI.TextBlock("Assigning different background colors based on state levels (0 to 3)."), hourPicker, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dynamic/Calculated Grid"), tss.UI.TextBlock("Using GridPicker for a complex logic visualization (Game of Life)."), Tesserae.Tests.Samples.GridPickerSample.GetGameOfLifeSample()])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.GridSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var grid = tss.UI.Grid(System.Array.init([tss.usX.fr$1(1), tss.usX.fr$1(1), tss.usX.px$1(200)], tss.us));
                grid.Gap(tss.usX.px$1(8));
                grid.Add(tss.ICX.GridRow(tss.Button, tss.ICX.GridColumnStretch(tss.Button, tss.ICX.WS(tss.Button, tss.UI.Button$1().SetText("Stretched Item")).Primary()), 1, 2));
                System.Linq.Enumerable.range(1, 10).forEach(function (v) {
                    grid.Add(tss.UI.Button$1().SetText(System.String.format("Item {0}", [H5.box(v, System.Int32)])));
                });

                var gridAutoSize = tss.UI.Grid([new tss.us.$ctor2("repeat(auto-fit, minmax(min(200px, 100%), 1fr))")]);
                gridAutoSize.Gap(tss.usX.px$1(8));
                System.Linq.Enumerable.range(1, 10).forEach(function (v) {
                    gridAutoSize.Add(tss.UI.Card(tss.ITFX.TextCenter(tss.txt, tss.UI.TextBlock(System.String.format("Responsive Item {0}", [H5.box(v, System.Int32)])))));
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.GridSample, 4439, "A component to display a grid"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The Grid component provides a powerful layout system based on CSS Grid. It allows you to define columns, rows, and gaps between items."), tss.UI.TextBlock("Items within a Grid can be explicitly positioned or stretched across multiple tracks, offering full control over complex 2D layouts.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Grid for page-level layouts or complex component structures where both rows and columns need coordination. For simple one-dimensional layouts (horizontal or vertical), consider using HStack or VStack instead. Leverage 'fr' units for flexible columns that fill available space proportionally. Use 'auto-fit' or 'auto-fill' with 'minmax' to create responsive grids that adapt to different screen sizes without media queries.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Fixed and Flexible Columns"), tss.UI.TextBlock("This grid uses two flexible columns (1fr) and one fixed column (200px). The first item is stretched across all columns."), grid, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Responsive Auto-fit Grid"), tss.UI.TextBlock("This grid automatically adjusts the number of columns based on the available width (min 200px per item)."), gridAutoSize])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.HorizontalSeparatorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.HorizontalSeparatorSample, 2991, "A visual separator for components"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A HorizontalSeparator visually divides content into groups. It can optionally contain text or other components to label the group it introduces."), tss.UI.TextBlock("The content can be aligned to the left, center, or right of the line.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use separators to provide structure to long forms or pages. Keep labels short and concise. Use them sparingly; too many separators can clutter the UI. Ensure the labels accurately describe the section that follows.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Text Alignment"), tss.UI.HorizontalSeparator("Center Aligned (Default)"), tss.UI.HorizontalSeparator("Left Aligned").Left(), tss.UI.HorizontalSeparator("Right Aligned").Right(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Themed and Custom Content"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.HorizontalSeparator("Primary Color").Primary(), tss.UI.HorizontalSeparator$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.PaddingRight(tss.Icon, tss.UI.Icon$2(2520), tss.usX.px$1(8)), tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Information Section"))])).Primary().Left()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Empty Separator"), tss.UI.TextBlock("A simple line without any label:"), tss.UI.HorizontalSeparator("")])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.HorizontalSplitViewSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var basicSplit = tss.ICX.H(tss.HorizontalSplitView, tss.ICX.WS(tss.HorizontalSplitView, tss.UI.HorizontalSplitView().Top(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral100), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Top Pane")))])).Bottom(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral200), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Bottom Pane")))]))), 300);

                var resizableSplit = tss.ICX.H(tss.HorizontalSplitView, tss.ICX.WS(tss.HorizontalSplitView, tss.UI.HorizontalSplitView().Top(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral100), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Editor")))])).Bottom(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral200), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Preview")))])).Resizable(function (height) {
                    tss.UI.Toast().Information(System.String.format("Top pane resized to {0}px", [H5.box(height, System.Int32)]));
                })), 300);

                var topSmaller = tss.ICX.H(tss.HorizontalSplitView, tss.ICX.WS(tss.HorizontalSplitView, tss.UI.HorizontalSplitView().Top(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Blue100), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Fixed Header (80px)")))])).Bottom(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral200), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Grows to fill")))])).TopIsSmaller(tss.usX.px$1(80))), 200);

                var bottomSmaller = tss.ICX.H(tss.HorizontalSplitView, tss.ICX.WS(tss.HorizontalSplitView, tss.UI.HorizontalSplitView().Top(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral200), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Grows to fill")))])).Bottom(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Blue100), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Fixed Footer (80px)")))])).BottomIsSmaller(tss.usX.px$1(80))), 200);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.HorizontalSplitViewSample, 4230, "Divides space into top and bottom panes with an optional resizable divider"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("HorizontalSplitView divides a container into a top pane and a bottom pane separated by a thin splitter bar. It is well-suited for editor/preview layouts, terminal-style interfaces, or any surface where two stacked areas need to coexist."), tss.UI.TextBlock("The divider can be made interactive so users can drag it to redistribute space between the two panes at runtime.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Set an explicit height on the split container to prevent it from collapsing. Use TopIsSmaller or BottomIsSmaller when one pane should be compact and the other should grow to fill available space. Enable Resizable only when user preference for the split ratio is genuinely useful \u2014 unnecessary resize handles add visual noise. Provide meaningful minimum sizes via CSS or layout constraints so neither pane can be dragged to zero.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Non-Resizable Split"), tss.ICX.MB(tss.HorizontalSplitView, basicSplit, 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Resizable Split (drag the divider)"), tss.ICX.MB(tss.HorizontalSplitView, resizableSplit, 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Top Pane Fixed, Bottom Grows"), tss.ICX.MB(tss.HorizontalSplitView, topSmaller, 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Bottom Pane Fixed, Top Grows"), bottomSmaller])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.InfiniteScrollingListSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var page = 1;
                var pageGrid = 1;

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.InfiniteScrollingListSample, 2773, "A list that scrolls infinitely"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("InfiniteScrollingList provides a way to render large sets of items by loading them on demand as the user scrolls. It uses a visibility sensor to detect when the end of the list is reached."), tss.UI.TextBlock("This approach is great for social feeds, search results, or any collection where you want to avoid explicit pagination buttons.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use infinite scrolling for content that is explored discovery-style rather than searched for specifically. Ensure that the loading state is clearly indicated to the user. Consider the performance impact of adding many DOM elements; for extremely large lists, VirtualizedList may be more appropriate. Provide a way for users to reach the footer of the page if necessary, perhaps by offering a 'Load More' button instead of fully automatic scrolling if the footer contains important links.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Vertical Infinite List"), tss.UI.TextBlock("Items are loaded 20 at a time with a small delay to simulate network latency."), tss.ICX.MB(tss.InfiniteScrollingList, tss.ICX.Height(tss.InfiniteScrollingList, tss.UI.InfiniteScrollingList$2(this.GetSomeItems(20, 0, " (Initial Set)"), H5.fn.bind(this, function () {
                    var $tcs = new H5.TCS();
                    (async () => (await H5.toPromise(this.GetSomeItemsAsync(20, H5.identity(page, ((page = (page + 1) | 0)))))))().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                })), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Grid-based Infinite List"), tss.UI.TextBlock("Displaying items in a 3-column grid that expands as you scroll."), tss.ICX.Height(tss.InfiniteScrollingList, tss.UI.InfiniteScrollingList$2(this.GetSomeItems(20, 0, " (Initial Set)"), H5.fn.bind(this, function () {
                    var $tcs = new H5.TCS();
                    (async () => (await H5.toPromise(this.GetSomeItemsAsync(20, H5.identity(pageGrid, ((pageGrid = (pageGrid + 1) | 0)))))))().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }), [tss.usX.percent$1(33), tss.usX.percent$1(33), tss.usX.percent$1(34)]), tss.usX.px$1(400))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetSomeItems: function (count, page, txt) {
                if (page === void 0) { page = -1; }
                if (txt === void 0) { txt = ""; }
                var pageString = page > 0 ? System.String.format("Page {0}", [H5.box(page, System.Int32)]) : "";
                return System.Linq.Enumerable.range(1, count).select(function (n) {
                    return tss.ICX.MinWidth(tss.Card, tss.UI.Card(tss.txtX.NonSelectable(tss.txt, tss.UI.TextBlock(System.String.format("{0} - Item {1}{2}", pageString, H5.box(n, System.Int32), txt)))), tss.usX.px$1(200));
                }).ToArray(tss.Card);
            },
            GetSomeItemsAsync: function (count, page, txt) {
                var $tcs = new H5.TCS();
                (async () => {
                    {
                        if (page === void 0) { page = -1; }
                        if (txt === void 0) { txt = ""; }
                        (await H5.toPromise(System.Threading.Tasks.Task.delay(500)));
                        return this.GetSomeItems(count, page, txt);
                    }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                return $tcs.task;
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ItemsListSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var obsList = new (tss.ObservableList(tss.IC)).ctor();
                var vs = tss.UI.VisibilitySensor(H5.fn.bind(this, function (v) {
                    obsList.remove(v);
                    obsList.AddRange(this.GetSomeItems(10, " (Dynamic)"));
                    v.Reset();
                    obsList.add(v);
                }));
                obsList.AddRange(this.GetSomeItems(10, " (Initial)"));
                obsList.add(vs);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.ItemsListSample, 2773, "A list that displays items"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ItemsList is a versatile component for displaying collections of items. It supports static lists, observable lists, and grid layouts."), tss.UI.TextBlock("It is ideal for smaller to medium-sized collections. For very large datasets, consider using VirtualizedList for better performance.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ItemsList when you want full control over the rendering of each item. Leverage observable lists to automatically update the UI when the underlying data changes. Use columns to create grid layouts that adapt to the container width. Always provide a meaningful empty message if there are no items to display. If you expect a very high number of items, ensure you test the performance or switch to a virtualized approach.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Simple List"), tss.ICX.MB(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), tss.usX.px$1(250)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Multi-column Grid"), tss.ICX.MB(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(12), [tss.usX.percent$1(33), tss.usX.percent$1(33), tss.usX.percent$1(34)]), tss.usX.px$1(300)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dynamic Observable List"), tss.UI.TextBlock("This list uses a VisibilitySensor to append more items as you scroll."), tss.ICX.MB(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList$1(obsList, [tss.usX.percent$1(50), tss.usX.percent$1(50)]), tss.usX.px$1(300)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Empty State"), tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(System.Array.init(0, null, tss.IC)).WithEmptyMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No items to show"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(150))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetSomeItems: function (count, suffix) {
                if (suffix === void 0) { suffix = ""; }
                return System.Linq.Enumerable.range(1, count).select(function (n) {
                    return tss.ICX.MinWidth(tss.Card, tss.UI.Card(tss.UI.TextBlock(System.String.format("Item {0}{1}", H5.box(n, System.Int32), suffix))), tss.usX.px$1(150));
                }).ToArray(tss.Card);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.KeyboardShortcutSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.KeyboardShortcutSample, 2612, "Render keyboard shortcuts as styled key chips"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("KeyboardShortcut renders one or more key names as styled chips that look like physical keyboard keys. It automatically adapts modifier key labels to the current OS (\u2318 on macOS, Ctrl on Windows/Linux) and handles special keys like Enter (\u21b5), Escape, and arrow keys.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use KeyboardShortcut inline inside tooltips, help text, and command-palette descriptions to make shortcuts easy to scan. Keep shortcuts short \u2014 a maximum of 3 keys is recommended.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Common Shortcuts"), tss.ICTX.Children$6(tss.S, tss.UI.VStack().Gap(tss.usX.px$1(12)), [tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Search"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Ctrl", "K"])]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Save"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Ctrl", "S"])]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Undo"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Ctrl", "Z"])]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Redo"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Ctrl", "Shift", "Z"])]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Copy"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Ctrl", "C"])]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Select all"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Ctrl", "A"])]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Close / Cancel"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Escape"])]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [tss.ITFX.Small(tss.txt, tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Confirm"), tss.usX.px$1(120))), tss.UI.KeyboardShortcut(["Enter"])])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Special Keys"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().Gap(tss.usX.px$1(16)).AlignItems("center"), [tss.UI.KeyboardShortcut(["ArrowUp"]), tss.UI.KeyboardShortcut(["ArrowDown"]), tss.UI.KeyboardShortcut(["ArrowLeft"]), tss.UI.KeyboardShortcut(["ArrowRight"]), tss.UI.KeyboardShortcut(["Tab"]), tss.UI.KeyboardShortcut(["Backspace"]), tss.UI.KeyboardShortcut(["Delete"])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Inline Usage"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(4)), [tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Press")), tss.UI.KeyboardShortcut(["Ctrl", "K"]), tss.ITFX.Small(tss.txt, tss.UI.TextBlock("to open the command palette, or")), tss.UI.KeyboardShortcut(["Escape"]), tss.ITFX.Small(tss.txt, tss.UI.TextBlock("to dismiss it."))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.LabelSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.LabelSample, 4462, "A component to display a label"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Labels provide a name or title for a component or a group of components. They are essential for accessibility and helping users understand the purpose of input fields."), tss.UI.TextBlock("While many Tesserae components have built-in labels, the standalone Label component offers more flexibility in positioning and styling.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use sentence casing for label text. Keep labels short and concise, typically using a noun or a short noun phrase. Do not use labels as instructional text; use TextBlocks or tooltips for that purpose. Ensure labels are positioned close to the components they describe. Use the 'Required' flag to clearly indicate mandatory fields.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard Label"), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required Label")), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Label")), tss.txtX.Primary(tss.Label, tss.UI.Label$1("Primary Colored Label"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Label with Content"), tss.UI.Label$1("Username").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your username")), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Inline Layouts"), tss.UI.TextBlock("Labels can be displayed inline with their content, with optional automatic width synchronization."), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Name").Inline().AutoWidth().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Department").Inline().AutoWidth().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Role").Inline().AutoWidth().SetContent(tss.UI.TextBox$1())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Right Aligned Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Short").Inline().AutoWidth(void 0, true).SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("A much longer label").Inline().AutoWidth(void 0, true).SetContent(tss.UI.TextBox$1())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.Label, tss.UI.Label$1("Small rounded"), tss.BR.Small), tss.ISX.Rounded(tss.Label, tss.UI.Label$1("Medium rounded"), tss.BR.Medium), tss.ISX.Rounded(tss.Label, tss.UI.Label$1("Full rounded"), tss.BR.Full)])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.LayerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var layer = tss.UI.Layer();
                var layer2 = tss.UI.Layer();
                var layerHost = tss.UI.LayerHost();

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.LayerSample, 151, "A component to display a layer"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Layer is a technical component used to render content outside of its parent's DOM tree, typically at the end of the document body. This allows content to escape boundaries like 'overflow: hidden' or complex z-index stacks, ensuring that elements like tooltips, context menus, and modals always appear on top of other content.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Layers for UI elements that must appear above all other content regardless of their position in the component hierarchy. Utilize 'LayerHost' when you need to project layered content into a specific part of the DOM instead of the default body location. Be mindful of the lifecycle of layered components to ensure they are properly removed from the DOM when no longer needed.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic layered content")), tss.LayerExtensions.Content(tss.Layer, layer, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("This is example layer content."), tss.UI.Button$1("Show second Layer").SetIcon$1(42).Primary().OnClick(function (s, e) {
                    layer2.IsVisible = true;
                }), tss.LayerExtensions.Content(tss.Layer, layer2, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("This is the second example layer content."), tss.UI.Button$1("Hide second Layer").SetIcon$1(1348).Primary().OnClick(function (s, e) {
                    layer2.IsVisible = false;
                })])), tss.UI.Toggle$1("Toggle Component Layer").OnChange(function (s, e) {
                    layer.IsVisible = s.IsChecked;
                }), tss.UI.Toggle()])), tss.UI.Toggle$1("Toggle Component Layer").OnChange(function (s, e) {
                    layer.IsVisible = s.IsChecked;
                }), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Using LayerHost to control projection")), tss.UI.Toggle$1("Show on Host").OnChange(function (s, e) {
                    layer.Host = s.IsChecked ? layerHost : null;
                }), layerHost])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.MarkdownBlockSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var staticSample = "# MarkdownBlock\r\n\r\n`MarkdownBlock` renders **Markdown** as sanitized HTML.\r\n\r\n- It is backed by the [marked](https://marked.js.org/) parser\r\n- The output is run through [DOMPurify](https://github.com/cure53/DOMPurify) before being inserted\r\n- A blockquote:\r\n\r\n> Markdown source goes in, safe HTML comes out.\r\n\r\n```csharp\r\nMarkdownBlock(\"# Hello\");\r\n```\r\n";

                var startingMarkdown = "## Try editing this!\r\n\r\nType some markdown below to see it rendered live.\r\n\r\n- **Bold**, *italic*, and ~~strike~~\r\n- Lists, [links](https://github.com/curiosity-ai/tesserae), and code\r\n\r\n| Feature  | Supported |\r\n| -------- | --------- |\r\n| GFM      | yes       |\r\n| Sanitize | yes       |\r\n";

                var live = tss.UI.MarkdownBlock(startingMarkdown);
                var editor = tss.ICX.H(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1(startingMarkdown)), 220).OnInput(function (ta, _) {
                    live.Text = ta.Text;
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.MarkdownBlockSample, 3275, "A component that renders Markdown as sanitized HTML"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("MarkdownBlock turns a Markdown source string into safely-rendered HTML. The bundled marked parser produces the HTML and DOMPurify strips anything unsafe before it ever reaches the DOM, so it is safe to feed user-authored content."), tss.UI.TextBlock("The component exposes a single Text property: assigning to it re-renders the output, which makes it easy to drive from a TextArea, an observable, or streamed assistant output.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Prefer MarkdownBlock over treating raw HTML strings as text - the sanitization step protects against script injection from third-party content. Keep MarkdownBlock inside a width-constrained container so wide tables and code blocks scroll instead of breaking the layout. For purely static labels, plain TextBlock is cheaper - reach for MarkdownBlock when the input genuinely contains Markdown.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Static Markdown"), tss.UI.MarkdownBlock(staticSample), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live editing"), tss.UI.TextBlock("Edit the Markdown on the left, the rendered output updates on the right."), tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), [tss.ICX.Grow(tss.TextArea, editor), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.UI.VStack()), 16), [live])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Sanitization"), tss.UI.TextBlock("MarkdownBlock will strip dangerous HTML even when it is embedded inside Markdown:"), tss.UI.MarkdownBlock("This `<script>alert('xss')</script>` will not run, and this <img src=x onerror=alert(1)> attribute is stripped too.")])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.MasonrySample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                var $t;
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.MasonrySample, 4439, "A masonry layout component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Masonry layout (also known as a Pinterest-style layout) is a grid where items are placed in optimal positions based on available vertical space."), tss.UI.TextBlock("Unlike a standard grid where rows have a uniform height, a Masonry layout allows items of varying heights to be packed tightly together.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Masonry for visually-driven content like image galleries or dashboard widgets with varying heights. Ensure that the number of columns is appropriate for the screen size. Provide a consistent gap between items to maintain a clean appearance. Avoid using Masonry for content that needs to be read in a specific sequential order, as the placement can be non-linear.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Masonry Grid (4 Columns)"), tss.ScrollBar.ScrollY(tss.Masonry, tss.ICTX.Children$6(tss.Masonry, tss.ICX.S(tss.Masonry, tss.UI.Masonry(4)), ($t = tss.IC, System.Linq.Enumerable.from(this.GetCards(50), $t).ToArray($t))))])).SetTitle("Usage")]), true, false, "");
            }
        },
        methods: {
            GetCards: function (count) {
                return new (H5.GeneratorEnumerable$1(tss.IC))(H5.fn.bind(this, function (count) {
                    var $s = 0,
                        $jff,
                        $rv,
                        rng,
                        i,
                        height,
                        $ae;

                    var $en = new (H5.GeneratorEnumerator$1(tss.IC))(H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                switch ($s) {
                                    case 0: {
                                        rng = new System.Random.ctor();
                                            i = 0;
                                            $s = 1;
                                            continue;
                                    }
                                    case 1: {
                                        if ( i < count ) {
                                                $s = 2;
                                                continue;
                                            }
                                        $s = 5;
                                        continue;
                                    }
                                    case 2: {
                                        height = (80 + H5.Int.mul(H5.Int.clip32((rng.NextDouble() * 4)), 40)) | 0;
                                            $en.current = tss.ICX.W$1(tss.Card, tss.ICX.H$1(tss.Card, tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.AlignCenter(tss.S, tss.UI.VStack()).JustifyContent("center"), [tss.UI.TextBlock(System.String.format("Card {0}", [H5.box(i, System.Int32)]))])), tss.usX.px$1(height)), tss.usX.percent$1(100));
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $s = 4;
                                        continue;
                                    }
                                    case 4: {
                                        i = (i + 1) | 0;
                                        $s = 1;
                                        continue;
                                    }
                                    case 5: {

                                    }
                                    default: {
                                        return false;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            throw $ae;
                        }
                    }));
                    return $en;
                }, arguments));
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.MessageSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.MessageSample, 1702, "A component to display a message"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The Message component is used to display static messages, alerts, or empty states. It supports an icon, title, text body, and an optional note area."), tss.UI.TextBlock("It comes with variants for standard, success, warning, and error states.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standard Message (with Note)"), tss.UI.Message("No Database Schema Yet", "Start by describing your database requirements in the chat. I'll help you design a complete schema with tables, relationships, and best practices.").Icon$1(1873).Note$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.ICX.PR(tss.Icon, tss.UI.Icon$2(719, "fi-rr-", "tss-fontsize-small", void 0), 8), tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Try saying: \"Create a blog database with users, posts, and comments\""))])), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Error Message"), tss.UI.Message("Something went wrong", "We couldn't save your changes. Please check your internet connection and try again.").Icon$1(1348).Variant(tss.MessageVariant.Error), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("No Results"), tss.UI.Message("No results found", "We couldn't find any items matching your search criteria.").Icon$1(3936).Variant(tss.MessageVariant.Default)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.MetricSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.MetricSample, 929, "A component to display a metric"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A Metric component displays a key value alongside a title and an optional indicator of change.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Metric to display important data points, such as requests, tokens, costs or errors. Keep titles short and clear. Combine with charts or grids to provide more context.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Metrics"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric("Requests", "1.1k").Change(tss.ISX.Foreground(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("")), tss.UI.Theme.Colors.Neutral600))), tss.usX.px$1(200)), tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric("Tokens", "196.97k")), tss.usX.px$1(200)), tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric("Cost", "$0.09")), tss.usX.px$1(200))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Metrics with Change Indicator"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric("Requests", "688.46k").Change(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ICX.S(tss.Icon, tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(189), tss.UI.Theme.Colors.Red600)), tss.ISX.Foreground(tss.txt, tss.UI.TextBlock("-0.4%"), tss.UI.Theme.Colors.Red600)]))), tss.usX.px$1(250)), tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric("Tokens", "10.57B").Change(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ICX.S(tss.Icon, tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(189), tss.UI.Theme.Colors.Red600)), tss.ISX.Foreground(tss.txt, tss.UI.TextBlock("-0.32%"), tss.UI.Theme.Colors.Red600)]))), tss.usX.px$1(250))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Card With Header & Tags"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.W$1(tss.Metric, tss.UI.Metric("Requests", "688.46k").Change(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ICX.S(tss.Icon, tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(189), tss.UI.Theme.Colors.Red600)), tss.ISX.Foreground(tss.txt, tss.UI.TextBlock("-0.4%"), tss.UI.Theme.Colors.Red600)])), tss.usX.px$1(250)), tss.ICX.W$1(tss.Metric, tss.UI.Metric("Tokens", "10.57B").Change(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ICX.S(tss.Icon, tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(189), tss.UI.Theme.Colors.Red600)), tss.ISX.Foreground(tss.txt, tss.UI.TextBlock("-0.32%"), tss.UI.Theme.Colors.Red600)])), tss.usX.px$1(250))])).SetTitle("Metrics")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Tooltips inside title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric$1(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ISX.Foreground(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Requests"))), tss.UI.Theme.Secondary.Foreground), tss.ICX.Tooltip$1(tss.Icon, tss.ICX.PL(tss.Icon, tss.ICX.S(tss.Icon, tss.UI.Icon$2(2520)), 4), "Total number of requests")]), tss.ITFX.SemiBold(tss.txt, tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock("1.1k"))))), tss.usX.px$1(200)), tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric$1(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ISX.Foreground(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Cost"))), tss.UI.Theme.Secondary.Foreground), tss.ICX.Tooltip$1(tss.Icon, tss.ICX.PL(tss.Icon, tss.ICX.S(tss.Icon, tss.UI.Icon$2(2520)), 4), "Total estimated cost")]), tss.ITFX.SemiBold(tss.txt, tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock("$0.09"))))), tss.usX.px$1(200))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Metrics with Sparkline Charts"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric("Web traffic", "1,234,567").Chart(tss.UI.Sparkline(System.Array.init([10, 20, 15, 30, 25, 40, 35, 50], System.Double))).Change(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ICX.S(tss.Icon, tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(229), tss.UI.Theme.Colors.Green600)), tss.ISX.Foreground(tss.txt, tss.UI.TextBlock("+12.3%"), tss.UI.Theme.Colors.Green600)]))), tss.usX.px$1(250)), tss.ICX.W$1(tss.Card, tss.UI.Card(tss.UI.Metric("Worker invocations", "14,352").Chart(tss.UI.Sparkline(System.Array.init([50, 45, 40, 48, 30, 20, 15, 10], System.Double), 100.0, 30.0, "var(--tss-danger-background-color)")).Change(tss.ICTX.Children$6(tss.S, tss.ICX.PT(tss.S, tss.UI.HStack().AlignItemsCenter(), 16), [tss.ICX.S(tss.Icon, tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(189), tss.UI.Theme.Colors.Red600)), tss.ISX.Foreground(tss.txt, tss.UI.TextBlock("-5.1%"), tss.UI.Theme.Colors.Red600)]))), tss.usX.px$1(250))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ModalSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var container = tss.UI.Raw$1();
                var modal = { };
                var lbl = { };
                tss.LayerExtensions.Content(tss.Modal, tss.ICX.Height(tss.Modal, tss.ICX.Width(tss.Modal, tss.UI.Var(tss.Modal, tss.UI.Modal("Sample Modal"), modal).LightDismiss(), tss.usX.vw$1(60)), tss.usX.vh$1(60)).SetFooter(tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("This is a footer note"))), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.TextBlock("Modals provide a focused environment for users to complete a task or view important information. They can be configured with various options like dark overlays, non-blocking behavior, and draggable headers."), tss.UI.Label$1("Light Dismiss").Inline().AutoWidth().SetContent(tss.UI.Toggle().OnChange(function (s, e) {
                    modal.v.CanLightDismiss = s.IsChecked;
                }).Checked(modal.v.CanLightDismiss)), tss.UI.Label$1("Is draggable").Inline().AutoWidth().SetContent(tss.UI.Toggle().OnChange(function (s, e) {
                    modal.v.IsDraggable = s.IsChecked;
                }).Checked(modal.v.IsDraggable)), tss.UI.Label$1("Is dark overlay").Inline().AutoWidth().SetContent(tss.UI.Toggle().OnChange(function (s, e) {
                    modal.v.IsDark = s.IsChecked;
                }).Checked(modal.v.IsDark)), tss.UI.Label$1("Is non-blocking").Inline().AutoWidth().SetContent(tss.UI.Toggle().OnChange(function (s, e) {
                    modal.v.IsNonBlocking = s.IsChecked;
                }).Checked(modal.v.IsNonBlocking)), tss.UI.Label$1("Hide close button").Inline().AutoWidth().SetContent(tss.UI.Toggle().OnChange(function (s, e) {
                    modal.v.WillShowCloseButton = !s.IsChecked;
                }).Checked(!modal.v.WillShowCloseButton)), tss.UI.Var(tss.Label, tss.UI.Label$1("Open a dialog from here"), lbl).SetContent(tss.UI.Button$1("Open").OnClick(function (s, e) {
                    tss.UI.Dialog("Dialog over Modal").Content(tss.UI.TextBlock("Hello World!")).YesNo(function () {
                        lbl.v.Text = "Yes";
                    }, function () {
                        lbl.v.Text = "No";
                    });
                }))]));

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ModalSample, 5061, "A modal dialog component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Modals are large overlays used for tasks that require a separate context, such as creating or editing complex entities, or for displaying rich content that shouldn't clutter the main interface. They provide more space than Dialogs and can host a variety of components.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Modals for multi-step tasks or content-heavy interactions. Ensure that the Modal has a clear title and provide multiple ways to dismiss it (e.g., Close button, clicking outside, or the Escape key). Use 'LightDismiss' for non-critical information and blocking behavior only when user input is essential. Always maintain a clear typographic hierarchy within the Modal content.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Button$1("Open Modal").OnClick(function (s, e) {
                    modal.v.Show();
                }), tss.UI.Button$1("Open Modal from top right").OnClick(function (s, e) {
                    modal.v.ShowAt(tss.usX.px$1(16), void 0, tss.usX.px$1(16), void 0);
                }), tss.UI.Button$1("Open Modal with minimum size").OnClick(function (s, e) {
                    tss.ICX.MinWidth(tss.Modal, tss.ICX.MinHeight(tss.Modal, tss.LayerExtensions.Content(tss.Modal, tss.UI.Modal$1().CenterContent().LightDismiss().Dark(), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("small content"))), tss.usX.vh$1(50)), tss.usX.vw$1(50)).Show();
                })])).SetTitle("Usage"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Button$1("Open Modal Below").OnClick(function (s, e) {
                    container.Content$1(tss.ICX.MinWidth(tss.Modal, tss.ICX.MinHeight(tss.Modal, tss.LayerExtensions.Content(tss.Modal, tss.UI.Modal("Embedded Modal").CenterContent().LightDismiss().Dark(), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("hosted small content"))), tss.usX.vh$1(30)), tss.usX.vw$1(50)).ShowEmbedded());
                }), container])).SetTitle("Embedded Modal")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.MonthPickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var minMonth = new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(((System.DateTime.getYear(System.DateTime.getToday()) - 1) | 0), 1);
                var maxMonth = new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(((System.DateTime.getYear(System.DateTime.getToday()) + 1) | 0), 12);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.MonthPickerSample, 782, "A control to select a year and month"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("MonthPicker lets users select a specific year and month using the browser's native month-input widget. It is ideal when day-level precision is unnecessary, such as selecting billing periods, report months, or subscription renewal dates."), tss.UI.TextBlock("The component surfaces the selected value as a typed (year, month) tuple, removing the need for manual string parsing.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use MonthPicker when the unit of time is a calendar month rather than a specific day. Always pre-populate with a sensible default (e.g. the current month) so users can confirm quickly without additional input. Apply Min/Max constraints when only a limited historical or future range makes sense for your use-case.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic MonthPicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Current Month").SetContent(new tss.MonthPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), System.DateTime.getMonth(System.DateTime.getToday()))).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}-{1:D2}", H5.box(s.Month.Item1, System.Int32), H5.box(s.Month.Item2, System.Int32)));
                })), tss.UI.Label$1("No Default").SetContent(new tss.MonthPicker(null).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}-{1:D2}", H5.box(s.Month.Item1, System.Int32), H5.box(s.Month.Item2, System.Int32)));
                })), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(new tss.MonthPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), System.DateTime.getMonth(System.DateTime.getToday()))).Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Min / Max Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1(System.String.format("Range: {0}-{1:D2} to {2}-{3:D2}", H5.box(minMonth.Item1, System.Int32), H5.box(minMonth.Item2, System.Int32), H5.box(maxMonth.Item1, System.Int32), H5.box(maxMonth.Item2, System.Int32))).SetContent(new tss.MonthPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), System.DateTime.getMonth(System.DateTime.getToday()))).SetMin(minMonth.$clone()).SetMax(maxMonth.$clone()).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}-{1:D2}", H5.box(s.Month.Item1, System.Int32), H5.box(s.Month.Item2, System.Int32)));
                }))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), new tss.MonthPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), System.DateTime.getMonth(System.DateTime.getToday()))).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Month changed to {0}-{1:D2}", H5.box(s.Month.Item1, System.Int32), H5.box(s.Month.Item2, System.Int32)));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.NavbarSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var navbar = tss.UI.Sidebar().AsNavbar();

                navbar.AddHeader(new Tesserae.SidebarButton.$ctor14("brand", 3802, "My App").Primary());
                navbar.AddHeader(new Tesserae.SidebarButton.$ctor14("dashboard", 1411, "Dashboard"));

                navbar.AddContent(new Tesserae.SidebarButton.$ctor14("profile", 4799, "Profile"));
                navbar.AddContent(new Tesserae.SidebarButton.$ctor14("settings", 3974, "Settings"));
                navbar.AddContent(new Tesserae.SidebarSeparator("sep1"));
                navbar.AddContent(new Tesserae.SidebarButton.$ctor14("logout", 4051, "Logout"));

                navbar.AddFooter(new Tesserae.SidebarButton.$ctor14("footer", 2520, "About"));

                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.NavbarSample, 2871, "A navigation bar component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A Sidebar rendered as a Navbar. Header items are inline, others are in a drawer.")])).SetTitle("Overview"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.ICX.H$1(tss.S, tss.UI.VStack(), tss.usX.px$1(500)), [navbar, tss.ICX.Padding(tss.txt, tss.UI.TextBlock("Page Content below the navbar..."), tss.usX.px$1(16))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.NodeViewSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var nodeView = tss.ICX.S(tss.NodeView, tss.UI.NodeView());

                nodeView.Register(Tesserae.Tests.Samples.NodeViewSample.HelloWorldNode);
                nodeView.Register(Tesserae.Tests.Samples.NodeViewSample.ComplexNode);
                nodeView.Register(Tesserae.Tests.Samples.NodeViewSample.DynamicNode);

                var stateBuilder = tss.NodeView.State();

                var node1_id = System.Guid.NewGuid().toString();
                var node1_inp = System.Guid.NewGuid().toString();
                var node1_out = System.Guid.NewGuid().toString();


                stateBuilder.AddNode(node1_id, "Hello World", "My First Node", 100, 100, 200, function (nb) {
                    nb.AddInput("inp", node1_inp, "Hello").AddOutput("out", node1_out);
                });

                var node2_id = System.Guid.NewGuid().toString();
                var node2_text_id = System.Guid.NewGuid().toString();
                var node2_int_id = System.Guid.NewGuid().toString();
                var node2_num_id = System.Guid.NewGuid().toString();
                var node2_btn_id = System.Guid.NewGuid().toString();
                var node2_chk_id = System.Guid.NewGuid().toString();
                var node2_sel_id = System.Guid.NewGuid().toString();
                var node2_sld_id = System.Guid.NewGuid().toString();
                var node2_out_id = System.Guid.NewGuid().toString();

                stateBuilder.AddNode(node2_id, "Complex", "A Complex Node", 400, 100, 320, function (nb) {
                    nb.AddInput("text", node2_text_id, "World").AddInput("int", node2_int_id, H5.box(42, System.Int32)).AddInput("num", node2_num_id, H5.box(9.99, System.Double, System.Double.format, System.Double.getHashCode)).AddInput("btn", node2_btn_id, null).AddInput("chk", node2_chk_id, H5.box(true, System.Boolean, System.Boolean.toString)).AddInput("sel", node2_sel_id, "B").AddInput("sld", node2_sld_id, H5.box(0.75, System.Double, System.Double.format, System.Double.getHashCode)).AddOutput("out", node2_out_id);
                });

                stateBuilder.AddConnection(node1_out, node2_text_id);

                nodeView.SetState$1(stateBuilder.Build());


                var textArea = tss.ICX.Grow(tss.TextArea, tss.ICX.H(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1()), 10));

                nodeView.OnChange(function (v) {
                    textArea.Text = v.GetJsonState(true);
                });

                textArea.OnBlur(function (ta, ev) {
                    nodeView.SetState(ta.Text);
                });

                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.NodeViewSample, 4075, "A utility to display nodes"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("NodeView is a powerful utility for creating node-based visual editors and data flows. It allows you to define custom node types with various input and output interfaces, enabling users to build complex logic or data pipelines graphically.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use NodeView for scenarios where users need to define relationships or workflows. Keep node definitions logical and consistent. Provide descriptive names for inputs and outputs. Utilize dynamic nodes when the node structure needs to adapt based on its internal state or external data.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.WS(tss.SplitView, tss.ICX.H(tss.SplitView, tss.UI.SplitView().SplitInMiddle().Resizable(), 600)).Left(nodeView).Right(tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [tss.UI.Label$1("JSON State"), textArea]))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.NotificationCenterSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var unreadCount = new (tss.SettableObservableT(System.Int32))(3);

                var center = tss.UI.NotificationCenter().LoadItems(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            var $t;
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(500)));
                            return System.Array.init([($t = new tss.NotificationCenter.NotificationItem(), $t.Id = "1", $t.Title = "Deployment completed", $t.Message = "Production release v2.4.1 was deployed successfully.", $t.Timestamp = System.DateTime.addMinutes(System.DateTime.getNow(), -5), $t.Tone = "tss-notif-success", $t.IsRead = false, $t), ($t = new tss.NotificationCenter.NotificationItem(), $t.Id = "2", $t.Title = "High memory usage", $t.Message = "Server eu-west-1 is at 92% memory. Consider scaling.", $t.Timestamp = System.DateTime.addMinutes(System.DateTime.getNow(), -38), $t.Tone = "tss-notif-warning", $t.IsRead = false, $t), ($t = new tss.NotificationCenter.NotificationItem(), $t.Id = "3", $t.Title = "New team member", $t.Message = "Alice joined the Engineering team.", $t.Timestamp = System.DateTime.addHours(System.DateTime.getNow(), -2), $t.Tone = "tss-notif-info", $t.IsRead = false, $t), ($t = new tss.NotificationCenter.NotificationItem(), $t.Id = "4", $t.Title = "Backup failed", $t.Message = "Nightly backup for db-prod failed. Check logs.", $t.Timestamp = System.DateTime.addHours(System.DateTime.addDays(System.DateTime.getNow(), -1), -3), $t.Tone = "tss-notif-danger", $t.IsRead = true, $t), ($t = new tss.NotificationCenter.NotificationItem(), $t.Id = "5", $t.Title = "Report ready", $t.Message = "Monthly usage report for April is ready to download.", $t.Timestamp = System.DateTime.addDays(System.DateTime.getNow(), -2), $t.Tone = "tss-notif-info", $t.IsRead = true, $t)], tss.NotificationCenter.NotificationItem);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }).BadgeCount(unreadCount).OnMarkRead(function (id) {
                    var current = unreadCount.Value$1;
                    if (current > 0) {
                        unreadCount.Value$1 = (current - 1) | 0;
                    }
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.NotificationCenterSample, 411, "A bell button that opens a panel of recent notifications"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("NotificationCenter provides a bell icon with an unread count badge. Clicking it opens a side panel listing recent notifications grouped by date (Today, Yesterday, Earlier), with read/unread state, tone-coded dots, and a mark-all-read action."), tss.UI.TextBlock("Notifications are loaded asynchronously via the LoadItems callback, so the panel always shows the most recent state.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Keep notification messages concise \u2014 title + one sentence. Use the Tone to convey severity (Info, Success, Warning, Danger). Decrement the badge count as the user reads individual notifications. Use OnClearAll to reset the store when the user clears everything.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live Demo"), tss.ICX.MB(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Click the bell icon to open the notification panel.")), 12), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(16)), [center, tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("\u2190 Click the bell")), tss.UI.Theme.Secondary.Foreground)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Badge Control"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(8)), [tss.UI.Button$1("Add notification").OnClick$1(function () {
                    var $t;
                    H5.identity(unreadCount.Value$1, (($t = (unreadCount.Value$1 + 1) | 0, unreadCount.Value$1 = $t, $t)));
                }), tss.UI.Button$1("Clear all").OnClick$1(function () {
                    unreadCount.Value$1 = 0;
                }), tss.UI.DeferSync$3(System.Int32, unreadCount, function (v) {
                    return tss.ICX.ML(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock(System.String.format("Unread: {0}", [H5.box(v, System.Int32)]))), 8);
                })])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.NumberPickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.NumberPickerSample, 4186, "A control to pick a number"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The NumberPicker provides an input field specifically for numeric values, leveraging the browser's native number input widget."), tss.UI.TextBlock("It supports constraints like minimum and maximum values, as well as step increments for easier value adjustment.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use the NumberPicker whenever a precise numeric input is required. Set appropriate 'min', 'max', and 'step' values to guide the user. If the range of numbers is small and discrete, consider using a Slider or ChoiceGroup instead. Use validation to ensure the entered number meets specific criteria (e.g., must be even or positive).")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic NumberPickers"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.NumberPicker()), tss.UI.Label$1("Initial value (42)").SetContent(tss.UI.NumberPicker(42)), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.NumberPicker().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Between 0 and 100").SetContent(tss.UI.NumberPicker().SetMin(0).SetMax(100)), tss.UI.Label$1("Step increment of 5").SetContent(tss.UI.NumberPicker().SetStep(5))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Must be an even number").SetContent(tss.vX.Validation(tss.NumberPicker, tss.UI.NumberPicker(), function (np) {
                    return np.Value % 2 === 0 ? null : "Please choose an even value";
                })), tss.UI.Label$1("Required field").SetContent(tss.UI.NumberPicker().Required())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.NumberPicker().OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Value changed to: {0}", [H5.box(s.Value, System.Int32)]));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ObservableStackSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            fields: {
                _elementIndex: 0,
                _stackElementsList: null
            },
            ctors: {
                init: function () {
                    this._elementIndex = 4;
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                Tesserae.Tests.Samples.ObservableStackSample._stackElementsList = new (tss.ObservableList(tss.ICID)).ctor();
                Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.ReplaceAll(System.Linq.Enumerable.range(0, 4).select(function (i) {
                    return new Tesserae.Tests.Samples.ObservableStackSample.StackElement(H5.toString(i), System.String.format("Item {0}", [H5.box(i, System.Int32)]));
                }).ToArray(Tesserae.Tests.Samples.ObservableStackSample.StackElement));

                var obsStack = new tss.OS(Tesserae.Tests.Samples.ObservableStackSample._stackElementsList, 0, true);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ObservableStackSample, 151, "A stack that observes changes"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ObservableStack is a specialized container that synchronizes its DOM with an observable list using an efficient reconciliation process."), tss.UI.TextBlock("Instead of re-rendering the entire list when a change occurs, it identifies which elements were added, removed, or moved by comparing their unique Identifiers and ContentHashes. This makes it ideal for high-performance lists where preserving scroll position or component state is important.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ObservableStack when your list data changes frequently or when you want smooth transitions for moved items. Ensure each item implements 'IComponentWithID' correctly, providing a stable 'Identifier' and a 'ContentHash' that reflects any changes in the item's data. Avoid frequent full-list replacements if only a few items have changed. Leverage the reconciliation behavior to keep the DOM footprint minimal and performance high.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Reconciliation Demo"), tss.UI.TextBlock("Modify the list below and watch how the 'Display Elements' on the right update efficiently."), tss.ICX.WS(tss.SplitView, tss.ICX.Height(tss.SplitView, tss.UI.SplitView(), tss.usX.px$1(500))).Left(tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1("Randomize Order").OnClick$1(function () {
                    Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.ReplaceAll(System.Linq.Enumerable.from(Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.Value, tss.ICID).orderBy(function (_) {
                            return Math.random();
                        }).toList(tss.ICID));
                }), tss.UI.Button$1("Add New Item").Primary().OnClick$1(H5.fn.bind(this, function () {
                    this.AddItem();
                }))]), 16), tss.ITFX.SemiBold(tss.Label, tss.UI.Label$1("Edit items in-place:")), tss.ICX.Grow(tss.IDefer, tss.UI.DeferSync$3(System.Collections.Generic.IReadOnlyList$1(tss.ICID), Tesserae.Tests.Samples.ObservableStackSample._stackElementsList, H5.fn.bind(this, function (elements) {
                    var list = tss.UI.VStack();
                    for (var i = 0; i < System.Array.getCount(elements, tss.ICID); i = (i + 1) | 0) {
                        var idx = { v : i };
                        var item = { v : H5.as(System.Array.getItem(elements, i, tss.ICID), Tesserae.Tests.Samples.ObservableStackSample.StackElement) };
                        list.Add(tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.UI.Button$1().SetIcon$1(229).OnClick$1((function ($me, idx) {
                            return H5.fn.bind($me, function () {
                                this.Move(idx.v, ((idx.v - 1) | 0));
                            });
                        })(this, idx)), tss.UI.Button$1().SetIcon$1(189).OnClick$1((function ($me, idx) {
                            return H5.fn.bind($me, function () {
                                this.Move(idx.v, ((idx.v + 1) | 0));
                            });
                        })(this, idx)), tss.ICX.WS(tss.TextBox, tss.ISX.Background(tss.TextBox, tss.UI.TextBox$1(item.v.DisplayName).OnBlur((function ($me, item, idx) {
                            return H5.fn.bind($me, function (tb, _) {
                                item.v.DisplayName = tb.Text;
                                this.Update(idx.v, item.v);
                            });
                        })(this, item, idx)), item.v.Color))]), 4));
                    }
                    return tss.ScrollBar.ScrollY(tss.S, list);
                })))]), 8)).Right(tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.SemiBold(tss.Label, tss.UI.Label$1("Rendered Stack:")), tss.ICX.WS(tss.OS, tss.ICX.H$1(tss.OS, obsStack, tss.usX.px$1(450)))]), 8))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Move: function (oldIdx, newIdx) {
                var $t;
                if (newIdx < 0 || newIdx >= Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.Count) {
                    return;
                }
                var list = ($t = tss.ICID, System.Linq.Enumerable.from(Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.Value, $t).toList($t));
                var item = list.getItem(oldIdx);
                list.removeAt(oldIdx);
                list.insert(newIdx, item);
                Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.ReplaceAll(list);
            },
            Update: function (idx, item) {
                var $t;
                var list = ($t = tss.ICID, System.Linq.Enumerable.from(Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.Value, $t).toList($t));
                list.setItem(idx, item);
                Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.ReplaceAll(list);
            },
            AddItem: function () {
                var $t;
                Tesserae.Tests.Samples.ObservableStackSample._elementIndex = (Tesserae.Tests.Samples.ObservableStackSample._elementIndex + 1) | 0;
                var list = ($t = tss.ICID, System.Linq.Enumerable.from(Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.Value, $t).toList($t));
                list.add(new Tesserae.Tests.Samples.ObservableStackSample.StackElement(H5.toString(Tesserae.Tests.Samples.ObservableStackSample._elementIndex), System.String.format("Item {0}", [H5.box(Tesserae.Tests.Samples.ObservableStackSample._elementIndex, System.Int32)])));
                Tesserae.Tests.Samples.ObservableStackSample._stackElementsList.ReplaceAll(list);
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.OmniBoxSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                var $t, $t1;
                this.$initialize();
                var attBtnForSearch = tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$2(3273), "Add attachment");
                var searchModeSample = tss.ICX.WS(tss.OmniBox, tss.UI.OmniBox(($t = new tss.OmniBox.Config(tss.OmniBox.Mode.Search), $t.PlaceholderSearch = "Type something like: potato AND ( tomato OR banana) AND NOT apple", $t.SearchFooter = ($t1 = new tss.OmniBox.FooterItems(), $t1.LeftSide = System.Array.init([tss.UI.Button$2(3802).OnClick$1(function () {
                    tss.UI.Toast().Success("Lift off \ud83d\ude80");
                })], tss.Button), $t1.RightSide = System.Array.init([attBtnForSearch], tss.Button), $t1), $t.SuggestionsFetcher = function (query) {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            if (System.String.isNullOrWhiteSpace(query)) {
                                return System.Array.init([], tss.OmniBox.OmniBoxSuggestionItem);
                            }

                            (await H5.toPromise(System.Threading.Tasks.Task.delay(150)));

                            var items = new (System.Collections.Generic.List$1(tss.OmniBox.OmniBoxSuggestionItem)).ctor();
                            var q = query.toLowerCase();

                            if (System.String.contains(("dataset / curiosity-prod"),q) || System.String.contains(("dataset / tesserae-docs"),q) || System.String.contains(("dataset / build-logs"),q)) {
                                items.add(new tss.OmniBox.OmniBoxSuggestionItem.$ctor1(tss.UI.TextBlock("dataset / curiosity-prod"), tss.UI.Icon$2(4439), tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(964), tss.UI.Theme.Primary.Foreground), null, "DATASETS"));
                                items.add(new tss.OmniBox.OmniBoxSuggestionItem.$ctor1(tss.UI.TextBlock("dataset / tesserae-docs"), tss.UI.Icon$2(4439), null, null, "DATASETS"));
                                items.add(new tss.OmniBox.OmniBoxSuggestionItem.$ctor1(tss.UI.TextBlock("dataset / build-logs"), tss.UI.Icon$2(4439), tss.ISX.Foreground(tss.Icon, tss.UI.Icon$2(964), tss.UI.Theme.Primary.Foreground), null, "DATASETS"));
                            }

                            items.add(new tss.OmniBox.OmniBoxSuggestionItem.$ctor1(tss.UI.TextBlock("a-model / Document.v3"), tss.UI.Icon$2(1551), null, null, "SCHEMAS"));
                            items.add(new tss.OmniBox.OmniBoxSuggestionItem.$ctor1(tss.UI.TextBlock("a-model / Embedding.v1"), tss.UI.Icon$2(1551), null, null, "SCHEMAS"));

                            return items.ToArray();
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }, $t))).WithHistory(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            return System.Array.init([tss.OmniBox.ParseQuery("apple"), tss.OmniBox.ParseQuery("orange"), tss.OmniBox.ParseQuery("tomato"), tss.OmniBox.ParseQuery("banana"), tss.OmniBox.ParseQuery("potato AND ( tomato OR banana) AND NOT apple")], tss.OmniBox.SearchQuery);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }).OnSearch(function (s, q) {
                    var $t2, $t3;
                    var snapInfo = q.Snaps != null && q.Snaps.length > 0 ? System.String.format(" \u2014 snaps: {0}", [H5.toArray(System.Linq.Enumerable.from(q.Snaps, tss.OmniBox.SnapHandler).select(function (sn) {
                                    return sn.SnapId;
                                })).join(", ")]) : "";
                    tss.UI.Toast().Information(System.String.format("Searched for: {0} (Parsed into {1} tokens){2}", q.RawQuery, H5.box(($t2 = (($t3 = q.Tokens) != null ? $t3.Count : null), $t2 != null ? $t2 : 0), System.Int32), snapInfo));
                }).RegisterSnaps([new tss.OmniBox.SnapHandler("docs", "Docs", System.Array.init(["docs", "documentation"], System.String), tss.UI.Icon$2(507), "Search the documentation"), new tss.OmniBox.SnapHandler("wiki", "Wikipedia", System.Array.init(["wiki", "wikipedia"], System.String), tss.UI.Icon$2(2140), "Search Wikipedia"), new tss.OmniBox.SnapHandler("code", "Code", System.Array.init(["code", "src", "source"], System.String), tss.UI.Icon$2(1873), "Search source code"), new tss.OmniBox.SnapHandler("ai", "AI Assist", System.Array.init(["ai", "ask"], System.String), tss.UI.Icon$2(2836), "Switch to AI search (exclusive)", void 0, void 0, true)]).SetKeyboardShortcut(["Ctrl", "K"]).SetSearchText("potato AND ( tomato OR banana) AND NOT apple");

                var fileDropAreaOnSearch = tss.UI.FileDropArea$1(searchModeSample).OnFilesDropped(function (s, files) {
                    tss.UI.Toast().Information(System.String.format("Dropped files on search box: {0}", [H5.toArray(System.Linq.Enumerable.from(files, File).select(function (f) {
                                    return f.name;
                                })).join(", ")]));
                }).SetAccepts("*");

                searchModeSample.InlineFilterChips.add(new tss.OmniBox.InlineFilterChip.ctor("Tag: Red", "var(--tss-danger-background-color)", "var(--tss-danger-foreground-color)"));
                searchModeSample.InlineFilterChips.add(new tss.OmniBox.InlineFilterChip.ctor("Author: Jules", void 0, void 0, function (_) {
                    tss.UI.Toast().Success("hi!");
                }, true));
                searchModeSample.InlineFilterChips.add(new tss.OmniBox.InlineFilterChip.$ctor1(tss.UI.Button$1("IComponent")));
                searchModeSample.SetSearchRightText("124 results");

                attBtnForSearch.OnClick(function (s, e) {
                    fileDropAreaOnSearch.OpenFileSelection();
                });

                var attBtnForChat = tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$2(3273), "Add attachment");

                var chatModeSample = tss.ICX.WS(tss.OmniBox, tss.UI.OmniBox(($t = new tss.OmniBox.Config(tss.OmniBox.Mode.Chat), $t.PlaceholderChat = "Ask me anything", $t.ChatFooter = ($t1 = new tss.OmniBox.FooterItems(), $t1.LeftSide = System.Array.init([tss.UI.Button$2(3802).OnClick$1(function () {
                    tss.UI.Toast().Success("Lift off \ud83d\ude80");
                })], tss.Button), $t1.RightSide = System.Array.init([attBtnForChat], tss.Button), $t1), $t))).SetModels$1([new tss.OmniBox.ModelOption("Opus 4.7"), new tss.OmniBox.ModelOption("Opus 4.7", "1M"), new tss.OmniBox.ModelOption("Sonnet 4.6"), new tss.OmniBox.ModelOption("Haiku 4.5")]).SetThinkingEffort(tss.OmniBox.ThinkingEffort.High).OnModelChanged(function (s, model, effort) {
                    tss.UI.Toast().Information(System.String.format("Selected {0} with {1} thinking effort", model.Name, H5.box(effort, tss.OmniBox.ThinkingEffort, System.Enum.toStringFn(tss.OmniBox.ThinkingEffort))));
                }).OnChat(function (s, q) {
                    s.IsGenerating = true;
                    window.setTimeout(function (_) {
                        if (s.IsGenerating) {
                            s.IsGenerating = false;
                            tss.UI.Toast().Information(q.Text);
                        }
                    }, 5000);
                }).OnStop(function (s) {
                    s.IsGenerating = false;
                });

                var lockedModel = new tss.OmniBox.ModelOption("Sonnet 4.6");
                var lockedChatModeSample = tss.ICX.WS(tss.OmniBox, tss.UI.OmniBox(($t = new tss.OmniBox.Config(tss.OmniBox.Mode.Chat), $t.PlaceholderChat = "This chat has a locked model", $t))).LockModel(lockedModel).SetThinkingEffort(tss.OmniBox.ThinkingEffort.Medium);

                var fileDropAreaOnChat = tss.UI.FileDropArea$1(chatModeSample).OnFilesDropped(function (s, files) {
                    tss.UI.Toast().Information(System.String.format("Dropped files on chat box: {0}", [H5.toArray(System.Linq.Enumerable.from(files, File).select(function (f) {
                                    return f.name;
                                })).join(", ")]));
                }).SetAccepts("*");

                attBtnForChat.OnClick(function (s, e) {
                    fileDropAreaOnChat.OpenFileSelection();
                });

                var searchAndChatModeSample = tss.ICX.WS(tss.OmniBox, tss.UI.OmniBox(($t = new tss.OmniBox.Config(tss.OmniBox.Mode.SearchAndChat), $t.PlaceholderChat = "Ask me anything", $t.PlaceholderSearch = "Search for anything", $t.ChatFooter = ($t1 = new tss.OmniBox.FooterItems(), $t1.LeftSide = System.Array.init([tss.ICX.ML(tss.Dropdown, tss.UI.Dropdown(), 16).Searchable().Items$1([tss.UI.DropdownItem$1("Consult Documents", "", 507).Selected(), tss.UI.DropdownItem$1("Find a flight", "", 71), tss.UI.DropdownItem$1("Book a hotel", "", 2431)])], tss.IC), $t1), $t))).OnSearch(function (s, q) {
                    var $t2, $t3;
                    tss.UI.Toast().Information(System.String.format("Searched for: {0} (Parsed into {1} tokens)", q.RawQuery, H5.box(($t2 = (($t3 = q.Tokens) != null ? $t3.Count : null), $t2 != null ? $t2 : 0), System.Int32)));
                }).OnChat(function (s, q) {
                    s.IsGenerating = true;
                    window.setTimeout(function (_) {
                        if (s.IsGenerating) {
                            s.IsGenerating = false;
                            tss.UI.Toast().Information(q.Text);
                        }
                    }, 5000);
                }).OnStop(function (s) {
                    s.IsGenerating = false;
                }).WithHistory(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            return System.Array.init(0, null, tss.OmniBox.SearchQuery);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }).SetKeyboardShortcut(["Ctrl", "Shift", "K"]);


                var toggle = tss.UI.Toggle$1("Disabled").OnChange(function (s, e) {
                    var disabled = s.IsChecked;
                    if (disabled) {
                        searchModeSample.Disabled();
                        chatModeSample.Disabled();
                        searchAndChatModeSample.Disabled();
                    } else {
                        searchModeSample.Disabled(false);
                        chatModeSample.Disabled(false);
                        searchAndChatModeSample.Disabled(false);
                    }
                });

                var snapAndFilterSample = tss.ICX.WS(tss.OmniBox, tss.UI.OmniBox(($t = new tss.OmniBox.Config(tss.OmniBox.Mode.Search), $t.PlaceholderSearch = "Type @ for snaps, or 'ext:' / 'filetype:' / 'lang:' for filter values", $t))).RegisterSnaps([new tss.OmniBox.SnapHandler("docs", "Docs", System.Array.init(["docs", "documentation"], System.String), tss.UI.Icon$2(507), "Search the documentation"), new tss.OmniBox.SnapHandler("wiki", "Wikipedia", System.Array.init(["wiki", "wikipedia"], System.String), tss.UI.Icon$2(2140), "Search Wikipedia"), new tss.OmniBox.SnapHandler("code", "Code", System.Array.init(["code", "src", "source"], System.String), tss.UI.Icon$2(1873), "Search source code")]).RegisterFilterSnaps([new tss.OmniBox.FilterSnapHandler.$ctor1("ext", "File extension", System.Array.init(["ext", "filetype"], System.String), System.Array.init(["cs", "ts", "tsx", "js", "jsx", "json", "md", "css", "html", "py", "rb", "go", "rs", "java", "kt", "swift", "yml", "yaml", "xml"], System.String), tss.UI.Icon$2(1873), "Filter results by file extension", void 0, void 0), new tss.OmniBox.FilterSnapHandler.ctor("lang", "Language", System.Array.init(["lang", "language"], System.String), function (input) {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(120)));
                            var all = System.Array.init(["csharp", "typescript", "javascript", "python", "ruby", "go", "rust", "java", "kotlin", "swift", "html", "css"], System.String);
                            if (System.String.isNullOrEmpty(input)) {
                                return all;
                            }
                            return System.Linq.Enumerable.from(all, System.String).where(function (v) {
                                    return System.String.indexOf(v, input, 0, null, 5) >= 0;
                                }).ToArray(System.String);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }, tss.UI.Icon$2(2140), "Filter results by language (async)", void 0, void 0)]).OnSearch(function (s, q) {
                    var snapInfo = q.Snaps != null && q.Snaps.length > 0 ? System.String.format(" \u2014 snaps: {0}", [H5.toArray(System.Linq.Enumerable.from(q.Snaps, tss.OmniBox.SnapHandler).select(function (sn) {
                                    return sn.SnapId;
                                })).join(", ")]) : "";
                    var filterInfo = q.FilterSnaps != null && q.FilterSnaps.length > 0 ? System.String.format(" \u2014 filters: {0}", [H5.toArray(System.Linq.Enumerable.from(q.FilterSnaps, tss.OmniBox.FilterSnap).select(function (f) {
                                    return (f.FilterId || "") + "=" + (f.Value || "");
                                })).join(", ")]) : "";
                    tss.UI.Toast().Information(System.String.format("Searched for: {0}{1}{2}", q.RawQuery, snapInfo, filterInfo));
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.OmniBoxSample, 3936, "An omnibox search component"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.MB(tss.Toggle, toggle, 16), tss.UI.TextBlock("Omnibox provides a powerful input field for switching between a chat and a search interaction. For search, it also provides support for parsing and visual rendering of logical operators like AND, OR, NOT, parenthesis, and quotes.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Modes"), tss.UI.Label$1("Search (with FileDropArea)").SetContent(tss.ICX.WS(tss.FileDropArea, fileDropAreaOnSearch)), tss.ICX.MT(tss.Label, tss.UI.Label$1("Chat").SetContent(tss.ICX.WS(tss.FileDropArea, fileDropAreaOnChat)), 6), tss.ICX.MT(tss.Label, tss.UI.Label$1("Chat with locked model").SetContent(tss.ICX.WS(tss.OmniBox, lockedChatModeSample)), 6), tss.ICX.MT(tss.Label, tss.UI.Label$1("Search & Chat").SetContent(tss.ICX.WS(tss.OmniBox, searchAndChatModeSample)), 6)])).SetTitle("Usage")])), tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Snaps and filter snaps"), tss.ICX.MB(tss.txt, tss.UI.TextBlock("Type @ to insert a snap (e.g. @docs, @wiki, @code). Type 'ext:', 'filetype:', or 'lang:' to autocomplete filter values \u2014 when committed they become removable chips. Both snap and filter selections are passed through with the search query."), 8), tss.UI.Label$1("Snaps + filter snaps").SetContent(tss.UI.Class(tss.OmniBox, snapAndFilterSample, "tss-omnibox-snap-and-filter-sample"))])).SetTitle("Snaps & Filter Snaps")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.OverflowSetSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.OverflowSetSample, 2944, "A component to display an overflow set"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("OverflowSet is a container that automatically moves items that don't fit into the available space into an overflow menu."), tss.UI.TextBlock("It is commonly used for command bars, navigation menus, or any list of actions where you want to maximize the visibility of primary items while ensuring all items are accessible.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use OverflowSet when you have a horizontal list of items that might exceed the screen width. Order items by importance so that the most critical actions are the last to be moved to the overflow menu. Provide a clear icon or label for the overflow trigger (usually a 'more' icon). Ensure that items in the overflow menu remain fully functional.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic OverflowSet"), tss.UI.TextBlock("Resize the window or container to see items moving into the '...' menu."), tss.ICX.PB(tss.OverflowSet, tss.UI.OverflowSet().Items([tss.UI.Button$1("Action 1").Link().OnClick(function (s, e) {
                    tss.UI.Toast().Information("Action 1");
                }), tss.UI.Button$1("Action 2").Link().OnClick(function (s, e) {
                    tss.UI.Toast().Information("Action 2");
                }), tss.UI.Button$1("Action 3").Link().OnClick(function (s, e) {
                    tss.UI.Toast().Information("Action 3");
                }), tss.UI.Button$1("Action 4").Link().OnClick(function (s, e) {
                    tss.UI.Toast().Information("Action 4");
                }), tss.UI.Button$1("Action 5").Link().OnClick(function (s, e) {
                    tss.UI.Toast().Information("Action 5");
                }), tss.UI.Button$1("Action 6").Link().OnClick(function (s, e) {
                    tss.UI.Toast().Information("Action 6");
                })]), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Icons and Constraints"), tss.ICX.PB(tss.OverflowSet, tss.ICX.MaxWidth(tss.OverflowSet, tss.UI.OverflowSet(), tss.usX.px$1(300)).Items([tss.UI.Button$1("Edit").SetIcon$1(1663).Link(), tss.UI.Button$1("Share").SetIcon$1(3981).Link(), tss.UI.Button$1("Delete").SetIcon$1(4675).Link(), tss.UI.Button$1("Copy").SetIcon$1(1303).Link(), tss.UI.Button$1("Move").SetIcon$1(243).Link()]), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Overflow Index"), tss.UI.TextBlock("Force overflow to start after the first item:"), tss.ICX.MaxWidth(tss.OverflowSet, tss.UI.OverflowSet().SetOverflowIndex(0), tss.usX.px$1(400)).Items([tss.UI.Button$1("Always Visible").Primary(), tss.UI.Button$1("Option A").Link(), tss.UI.Button$1("Option B").Link(), tss.UI.Button$1("Option C").Link()])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PaginationSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var status = tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Showing page 1"));

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.PaginationSample, 120, "A component to navigate through pages"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Pagination allows users to navigate through a large set of data by breaking it into smaller, manageable chunks called pages."), tss.UI.TextBlock("It provides controls to move between pages, jump to specific pages, and see the current position within the total set.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use pagination when you have a large amount of content that would be overwhelming or slow to load all at once. Clearly show the total number of items and the current page. Provide 'Previous' and 'Next' controls for sequential navigation. If the number of pages is high, consider using a simplified view or allowing the user to jump to the first/last page. Keep the pagination controls in a consistent location, typically at the bottom of the content area.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Pagination"), tss.ICX.MB(tss.Card, tss.UI.Card(status), 16), tss.UI.Pagination(120, 10, 1).OnPageChange(function (p) {
                    status.Text = System.String.format("Showing page {0}", [H5.box(p.CurrentPage, System.Int32)]);
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Small Result Set"), tss.UI.Pagination(25, 10, 1).OnPageChange(function (p) {
                    tss.UI.Toast().Information(System.String.format("Selected page {0}", [H5.box(p.CurrentPage, System.Int32)]));
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Large Result Set"), tss.UI.Pagination(1000, 20, 5).OnPageChange(function (p) {
                    tss.UI.Toast().Information(System.String.format("Selected page {0}", [H5.box(p.CurrentPage, System.Int32)]));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PanelSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var panel = tss.UI.Panel().LightDismiss();
                tss.LayerExtensions.Content(tss.Panel, panel, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ITFX.SemiBold(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock("Sample panel"))), tss.UI.ChoiceGroup("Side:").Choices([tss.UI.Choice("Far").Selected().OnSelected(function (x) {
                    panel.Side = "tss-panelSide-far";
                }), tss.UI.Choice("Near").OnSelected(function (x) {
                    panel.Side = "tss-panelSide-near";
                })]), tss.UI.Toggle$1("Light Dismiss").OnChange(function (s, e) {
                    panel.CanLightDismiss = s.IsChecked;
                }).Checked(panel.CanLightDismiss), tss.UI.ChoiceGroup("Size:").Choices([tss.UI.Choice("Small").Selected().OnSelected(function (x) {
                    panel.Size = "tss-panelSize-small";
                }), tss.UI.Choice("Medium").OnSelected(function (x) {
                    panel.Size = "tss-panelSize-medium";
                }), tss.UI.Choice("Large").OnSelected(function (x) {
                    panel.Size = "tss-panelSize-large";
                }), tss.UI.Choice("LargeFixed").OnSelected(function (x) {
                    panel.Size = "tss-panelSize-largefixed";
                }), tss.UI.Choice("ExtraLarge").OnSelected(function (x) {
                    panel.Size = "tss-panelSize-extralarge";
                }), tss.UI.Choice("FullWidth").OnSelected(function (x) {
                    panel.Size = "tss-panelSize-fullwidth";
                })]), tss.UI.Toggle$1("Is non-blocking").OnChange(function (s, e) {
                    panel.IsNonBlocking = s.IsChecked;
                }).Checked(panel.IsNonBlocking), tss.UI.Toggle$1("Is dark overlay").OnChange(function (s, e) {
                    panel.IsDark = s.IsChecked;
                }).Checked(panel.IsDark), tss.UI.Toggle$1("Hide close button").OnChange(function (s, e) {
                    panel.ShowCloseButton = !s.IsChecked;
                }).Checked(!panel.ShowCloseButton)])).SetFooter(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1("Footer Button 1").Primary(), tss.UI.Button$1("Footer Button 2")]));

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.PanelSample, 5063, "A panel component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Panels are sliding overlays typically used for creation or management tasks, such as editing a user's profile or configuring settings. They provide a large, temporary surface that slides in from either the left or right side of the screen, keeping the user within the current context while providing space for complex forms or information.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Panels for self-contained tasks that are too large for a Dialog or Modal. Choose the 'Far' side (right) for most common actions, and 'Near' (left) for navigation-related content. Provide clear 'Save' and 'Cancel' actions in the footer. Ensure that the Panel size is appropriate for its content, using wider variants for complex forms. Use 'LightDismiss' to allow users to quickly exit by clicking outside the panel.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Button$1("Open panel").OnClick(function (s, e) {
                    panel.Show();
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.PickerSample, 1378, "A control to pick an item"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Pickers are used to select one or more items, such as people or tags, from a large list. They provide a search-based interface with suggestions."), tss.UI.TextBlock("This component is highly flexible, allowing for custom item rendering, single or multiple selections, and different suggestion behaviors.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Pickers when the number of options is too large for a standard Dropdown. Ensure that the items can be easily searched by text. Use clear icons or visual indicators if it helps users identify the correct item quickly. For multiple selections, consider how the selected items will be displayed\u2014either inline or in a separate list. Provide a helpful 'suggestions title' to guide the user when they interact with the picker.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Multi-selection Picker"), tss.UI.TextBlock("Allows selecting multiple tags from the suggestions."), tss.ICX.MB(tss.Picker(Tesserae.Tests.Samples.PickerSampleItem), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItem, 2147483647, false, 0, true, "Suggested Names").Items(this.GetPickerItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Single Selection Picker"), tss.UI.TextBlock("Limits selection to only one item at a time."), tss.ICX.MB(tss.Picker(Tesserae.Tests.Samples.PickerSampleItem), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItem, 1, false, 0, true, "Select one").Items(this.GetPickerItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Rendered Items"), tss.UI.TextBlock("Using icons and complex components for both suggestions and selections."), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItemWithComponents, 2147483647, false, 0, false, "System Items").Items(this.GetComponentPickerItems())])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetPickerItems: function () {
                return System.Array.init([new Tesserae.Tests.Samples.PickerSampleItem("Bob"), new Tesserae.Tests.Samples.PickerSampleItem("BOB"), new Tesserae.Tests.Samples.PickerSampleItem("Donuts by J Dilla"), new Tesserae.Tests.Samples.PickerSampleItem("Donuts"), new Tesserae.Tests.Samples.PickerSampleItem("Coffee"), new Tesserae.Tests.Samples.PickerSampleItem("Chicken Coop"), new Tesserae.Tests.Samples.PickerSampleItem("Cherry Pie"), new Tesserae.Tests.Samples.PickerSampleItem("Chess"), new Tesserae.Tests.Samples.PickerSampleItem("Cooper")], Tesserae.Tests.Samples.PickerSampleItem);
            },
            GetComponentPickerItems: function () {
                return System.Array.init([new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Bob", 499), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("BOB", 463), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Donuts", 879), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Coffee", 1199), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Chess", 976), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Cooper", 2546)], Tesserae.Tests.Samples.PickerSampleItemWithComponents);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PivotSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.PivotSample, 151, "A navigation pivot"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Pivots are used for navigating between different views or categories of content within the same context. They provide a compact way to switch between related data sets, such as different tabs in a settings page or different views of a list.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Pivots to organize content into logical categories. Keep labels short and descriptive. Ensure that the most frequently used views are placed first. Utilize the 'Justified' or 'Centered' styles when the pivot should span the full width of its container. Use the 'Cached' option to preserve the state of a tab's content when switching away.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Normal Style"), this.GetPivot(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Justified Style"), this.GetPivot().Justified(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Centered Style"), this.GetPivot().Centered(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Cached vs. Not Cached Tabs"), tss.PivotX.Pivot(tss.PivotX.Pivot(tss.UI.Pivot(), "tab1", tss.UI.PivotTitle("Cached"), function () {
                    return tss.ITFX.Regular(tss.txt, tss.UI.TextBlock(System.DateTimeOffset.UtcNow.toString()));
                }, true, false, void 0), "tab2", tss.UI.PivotTitle("Not Cached"), function () {
                    return tss.ITFX.Regular(tss.txt, tss.UI.TextBlock(System.DateTimeOffset.UtcNow.toString()));
                }, false, false, void 0), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Cached vs. Not Cached Tabs"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Scroll with limited height"), tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.ICX.MaxHeight(tss.Pivot, tss.UI.Pivot(), tss.usX.px$1(500)), "tab1", tss.UI.PivotTitle("5 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab2", tss.UI.PivotTitle("10 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(20)), 16);
                }), true, false, void 0), "tab3", tss.UI.PivotTitle("50 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(50)), 16);
                }), true, false, void 0), "tab4", tss.UI.PivotTitle("100 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(100)), 16);
                }), true, false, void 0), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Tab Overflow"), tss.ICX.H(tss.SplitView, tss.ICX.WS(tss.SplitView, tss.UI.SplitView().Resizable()), 500).LeftIsSmaller(tss.usX.px$1(300)).Left(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.ICX.S(tss.Pivot, tss.UI.Pivot()), "tab1", tss.UI.PivotTitle("Tab 1"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab2", tss.UI.PivotTitle("Tab 2"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab3", tss.UI.PivotTitle("Tab 3"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab4", tss.UI.PivotTitle("Tab 4"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab5", tss.UI.PivotTitle("Tab 5"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab6", tss.UI.PivotTitle("Tab 6"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab7", tss.UI.PivotTitle("Tab 7"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0), "tab8", tss.UI.PivotTitle("Tab 8"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true, false, void 0)).Right(tss.txtX.BreakSpaces(tss.txt, tss.ICX.WS(tss.txt, tss.UI.TextBlock("\ud83d\udc48 resize this area to scroll the tab strip \u2014 use the chevrons, the mouse wheel, or arrow / Home / End keys, and click the \u22ef button for an All Tabs menu")))), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Many Tabs with Long Titles"), this.GetManyTabsPivot()])).SetTitle("Usage")]));
            }
        },
        methods: {
            GetManyTabsPivot: function () {
                var titles = System.Array.init(["Project Overview", "Recent Activity", "Pull Requests", "Code Review", "Build Pipelines", "Test Results", "Deployments", "Issue Tracker", "Documentation", "Team Members", "Performance Metrics", "Security Audits", "Release Notes", "Settings & Preferences", "Integrations", "Audit Log"], System.String);
                var pivot = tss.ICX.H(tss.Pivot, tss.UI.Pivot(), 220);
                for (var i = 0; i < titles.length; i = (i + 1) | 0) {
                    var idx = { v : i };
                    pivot = tss.PivotX.Pivot(pivot, System.String.format("many-{0}", [H5.box(idx.v, System.Int32)]), tss.UI.PivotTitle(titles[System.Array.index(idx.v, titles)]), (function ($me, idx) {
                        return function () {
                            return tss.ICX.P(tss.txt, tss.UI.TextBlock(System.String.format("Content for: {0}", [titles[System.Array.index(idx.v, titles)]])), 16);
                        };
                    })(this, idx), true, false, void 0);
                }
                return pivot;
            },
            GetPivot: function () {
                return tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.UI.Pivot(), "first-tab", tss.UI.PivotTitle("First Tab"), function () {
                    return tss.UI.TextBlock("First Tab");
                }), "second-tab", tss.UI.PivotTitle("Second Tab"), function () {
                    return tss.UI.TextBlock("Second Tab");
                }), "third-tab", tss.UI.PivotTitle("Third Tab"), function () {
                    return tss.UI.TextBlock("Third Tab");
                });
            },
            Render: function () {
                return this.content.tss$IC$Render();
            },
            GetSomeItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (number) {
                    return tss.ICX.MinWidth(tss.Card, tss.UI.Card(tss.txtX.NonSelectable(tss.txt, tss.UI.TextBlock(System.String.format("Lorem Ipsum {0}", [H5.box(number, System.Int32)])))), tss.usX.px$1(200));
                }).ToArray(tss.Card);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PivotSelectorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.PivotSelectorSample, 1378, "A control to select a pivot"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("PivotSelector is a variation of the Pivot component that uses a Dropdown for navigation. It is particularly effective for mobile-first designs or interfaces with a large number of tabs that would otherwise require excessive horizontal scrolling.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use PivotSelector when horizontal space is constrained or when the number of tabs is dynamic and potentially large. Provide clear icons and text for each tab to aid navigation. Utilize the 'SetCommands' feature to surface global actions relevant to all tabs, such as 'Add New' or 'Refresh'.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic PivotSelector"), tss.PivotSelectorX.Pivot$1(tss.PivotSelectorX.Pivot$1(tss.PivotSelectorX.Pivot$1(tss.UI.PivotSelector(), "tab1", "Tab 1", function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 1"), 32));
                }), "tab2", "Tab 2", function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 2"), 32));
                }), "tab3", "Tab 3", function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 3"), 32));
                }), tss.ICX.PT(tss.IC, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("PivotSelector with custom buttons"), 16), tss.PivotSelectorX.Pivot(tss.PivotSelectorX.Pivot(tss.UI.PivotSelector().SetCommands([tss.UI.Button$1().SetIcon$1(42).NoBorder().NoBackground().OnClick$1(function () {
                    alert("Add clicked");
                }), tss.UI.Button$1().SetIcon$1(3974).NoBorder().NoBackground().OnClick$1(function () {
                    alert("Settings clicked");
                })]), "tab1", function () {
                    return tss.UI.Button$1("Tab 1").NoBackground().NoBorder().SetIcon$1(3802);
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 1"), 32));
                }), "tab2", function () {
                    return tss.UI.Button$1("Tab 2").NoBackground().NoBorder().SetIcon$1(831);
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 2"), 32));
                }), tss.ICX.PT(tss.IC, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("PivotSelector with large number of tabs"), 16), tss.PivotSelectorX.Pivot$2(tss.UI.PivotSelector(), System.Linq.Enumerable.range(1, 20).select(function (i) {
                    return new (System.ValueTuple$3(System.String,System.String,Function)).$ctor1(System.String.format("tab{0}", [H5.box(i, System.Int32)]), System.String.format("Tab {0}", [H5.box(i, System.Int32)]), function () {
                        return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock(System.String.format("Content for Tab {0}", [H5.box(i, System.Int32)])), 32));
                    });
                }).ToArray(System.ValueTuple$3(System.String,System.String,Function)))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.PlanSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var plan1 = tss.ICX.MaxWidth(tss.Plan, tss.UI.Plan("SCIM user provisioning deep dive").HeaderCommands([tss.ISX.Rounded(tss.Button, tss.UI.Button$1("Update").NoBorder())]).AddTask("Collect official SCIM RFCs and specifications from IETF and RFC repositories.", true).AddTask("Survey major identity providers' SCIM documentation and authentication methods.", false).AddTask("Gather sample SCIM request and response payloads and endpoint patterns.", false).AddTask("Identify open-source SCIM implementations and C# libraries with examples.", false).AddTask("Compile technical explanation covering endpoints, auth, schemas, and examples.", false).FooterMessage("Finalizing details for licenses and attribution...").FooterCommands([tss.ITFX.SemiBold(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("117 searches")))]).Progress(1, 5).Stop(), tss.usX.px$1(800));

                var plan2 = tss.ICX.MaxWidth(tss.Plan, tss.UI.Plan("Database Migration Plan").AddTask("Backup database", true).AddTask("Run schema update", true).AddTask("Migrate data", true).FooterMessage("Migration complete").Progress$1(100).HideStartStopButton(), tss.usX.px$1(800));

                plan2.Render().style.maxWidth = "800px";

                var plan3 = tss.ICX.MaxWidth(tss.Plan, tss.UI.Plan("Analyzing Log Files").AddTask("Download logs from S3", true).AddTask("Parse JSON structure", true).AddTask("Find error patterns", false).FooterMessage("Scanning file 45 of 200...").Indeterminate().Start(), tss.usX.px$1(800));

                plan3.Render().style.maxWidth = "800px";

                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.PlanSample, 2869, "A component to display a plan"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The Plan component displays a complex task with its sub-tasks and overall progress.")])).SetTitle("Overview"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.PB(tss.txt, tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Default usage showing a running plan with partial progress.")), 16), 8), plan1, tss.ICX.PB(tss.txt, tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("A completed plan, with the stop button hidden.")), 16), 8), plan2, tss.ICX.PB(tss.txt, tss.ICX.PT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("A plan with indeterminate progress.")), 16), 8), plan3])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ProgressIndicatorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ProgressIndicatorSample, 4226, "A component to indicate progress"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ProgressIndicators provide visual feedback for operations that take more than a few seconds. They show the current completion status and help set expectations for how much work remains. If the total amount of work is unknown, use the indeterminate state or a Spinner instead.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use a ProgressIndicator when the total units to completion can be quantified. Provide a clear label describing the operation in progress. Use the indeterminate state only when the duration is unknown. Combine multiple related steps into a single progress bar for a smoother experience. Avoid letting progress appear to move backwards unless a step failed and is being retried.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("States")), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Empty").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(0), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("30%").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(30), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("60%").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(60), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Full").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(100), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Indeterminate").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Indeterminated(), tss.usX.px$1(400))))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ProgressModalSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var ProgressFrame = null;
                var PlayModal = null;
                var modal;

                var cts;

                var progress = 0;




                ProgressFrame = function (a) {
                    if (cts.isCancellationRequested) {
                        modal.ProgressSpin().Message("Cancelling...");
                        tss.tX.fireAndForget(System.Threading.Tasks.Task.delay(2000).continueWith(function (_) {
                            return modal.Hide();
                        }));
                        return;
                    }

                    progress++;
                    if (progress < 100) {
                        modal.Message(System.String.format("Processing {0}%", [H5.box(progress, System.Single, System.Single.format, System.Single.getHashCode)])).Progress$1(progress);
                        window.setTimeout(ProgressFrame, 16);
                    } else {
                        modal.Message("Finishing...").ProgressIndeterminated();
                        tss.tX.fireAndForget(System.Threading.Tasks.Task.delay(5000).continueWith(function (_) {
                            return modal.Hide();
                        }));
                    }
                };
                PlayModal = function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            modal = tss.UI.ProgressModal().Title("Lorem Ipsum");
                            cts = new System.Threading.CancellationTokenSource();
                            modal.WithCancel(function (b) {
                                b.Disabled();
                                cts.cancel();
                            });
                            progress = 0;
                            modal.Message("Preparing to process...").ProgressSpin().Show();
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(1500)));
                            window.setTimeout(ProgressFrame, 16);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                };

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ProgressModalSample, 4226, "A modal dialog for progress"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ProgressModal is a specialized modal overlay that combines a title, a message, and a progress indicator. It is used for long-running operations where it is important to block other user interactions until the task is complete, while keeping the user informed of the progress.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ProgressModal only for operations that truly require the user's focus and shouldn't be interrupted. Ensure that the message provides clear context for what is being processed. Always provide a way to cancel the operation if possible. For background tasks that don't need to block the entire UI, consider using an in-place ProgressIndicator or Spinner instead.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Button$1("Open Modal").OnClick(function (s, e) {
                    tss.tX.fireAndForget(PlayModal());
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ProgressRingSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var ring75 = tss.UI.ProgressRing(64, 6).Progress(75, 100).Label("75%");
                var ring50 = tss.UI.ProgressRing(64, 6).Progress(50, 100).Label("50%");
                var ring25 = tss.UI.ProgressRing(64, 6).Progress(25, 100).Label("25%");
                var animBtn = { };

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ProgressRingSample, 1014, "A circular progress indicator"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ProgressRing displays progress in a circular donut style. Use it alongside a metric value, in a dashboard card header, or to track quota/usage."), tss.UI.TextBlock("It supports determinate values (0\u2013100), an indeterminate spinning state, and an optional text label in the centre.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ProgressRing when horizontal space is limited and a compact indicator is preferred. For linear progress (e.g. file upload bars), use ProgressIndicator instead. Always include a textual label or aria-label so screen readers can convey the value.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Determinate Rings"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().Gap(tss.usX.px$1(24)).AlignItems("flex-end"), [tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [ring25, tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("25%")), 8)]), tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [ring50, tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("50%")), 8)]), tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [ring75, tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("75%")), 8)]), tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [tss.UI.ProgressRing(64, 6).Progress(100, 100).Label("100%"), tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("100%")), 8)])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Sizes"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().Gap(tss.usX.px$1(24)).AlignItems("flex-end"), [tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [tss.UI.ProgressRing(32, 3).Progress(60, 100), tss.ICX.MT(tss.txt, tss.ITFX.XSmall(tss.txt, tss.UI.TextBlock("Small")), 8)]), tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [tss.UI.ProgressRing(48, 4).Progress(60, 100), tss.ICX.MT(tss.txt, tss.ITFX.XSmall(tss.txt, tss.UI.TextBlock("Medium")), 8)]), tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [tss.UI.ProgressRing(64, 6).Progress(60, 100), tss.ICX.MT(tss.txt, tss.ITFX.XSmall(tss.txt, tss.UI.TextBlock("Large")), 8)]), tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItems("center"), [tss.UI.ProgressRing(96, 8).Progress(60, 100).Label("60%"), tss.ICX.MT(tss.txt, tss.ITFX.XSmall(tss.txt, tss.UI.TextBlock("XLarge")), 8)])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Indeterminate (Loading)"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().Gap(tss.usX.px$1(24)).AlignItems("center"), [tss.UI.ProgressRing(48, 4).Indeterminate(), tss.UI.ProgressRing(64, 6).Indeterminate(), tss.UI.ProgressRing(96, 8).Indeterminate().Label("\u2026")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Animated Fill"), tss.UI.Var(tss.Button, tss.UI.Button$1("Start 5s countdown").Primary(), animBtn).OnClickSpinWhile(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            animBtn.v.Disabled();
                            var liveRing = tss.UI.ProgressRing(64, 6).Label("0%");
                            var container = tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center").Gap(tss.usX.px$1(12)), [liveRing, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Processing\u2026"))]);
                            tss.UI.Toast().Information$2(container).Duration(System.TimeSpan.fromSeconds(7));
                            for (var i = 1; i <= 5; i = (i + 1) | 0) {
                                (await H5.toPromise(System.Threading.Tasks.Task.delay(1000)));
                                liveRing.Progress(i, 5).Label(System.String.format("{0}%", [H5.box(H5.Int.mul(i, 20), System.Int32)]));
                            }
                            animBtn.v.Disabled(false);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.RatingSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var selectedValue = new (tss.SettableObservableT(System.Int32))(0);
                var interactiveRating = tss.UI.Rating(5).SetValue(3).OnChange$1(function (v) {
                    selectedValue.Value$1 = v;
                    tss.UI.Toast().Information(v === 0 ? "Rating cleared" : System.String.format("Rated {0} star{1}", H5.box(v, System.Int32), (v === 1 ? "" : "s")));
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.RatingSample, 4324, "A component for collecting or displaying star ratings"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The Rating component lets users express a value judgment on a 1-to-N star scale. It supports interactive selection, read-only display, and custom star counts.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use ratings to collect feedback or signal quality. Provide a way to clear the rating (clicking the selected star again). Show the read-only version when displaying aggregate scores. Keep the star count at 5 unless your domain convention differs.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Rating"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [interactiveRating, tss.UI.DeferSync$3(System.Int32, selectedValue, function (v) {
                    return tss.ITFX.Small(tss.txt, tss.ICX.ML(tss.txt, tss.UI.TextBlock(v === 0 ? "Not rated" : System.String.format("{0} / 5", [H5.box(v, System.Int32)])), 12));
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Read-Only Ratings"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.UI.Rating(5).SetValue(5).ReadOnly(), tss.ITFX.Small(tss.txt, tss.ICX.ML(tss.txt, tss.UI.TextBlock("5.0 \u2013 Excellent"), 8))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.UI.Rating(5).SetValue(3).ReadOnly(), tss.ITFX.Small(tss.txt, tss.ICX.ML(tss.txt, tss.UI.TextBlock("3.0 \u2013 Average"), 8))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.UI.Rating(5).SetValue(1).ReadOnly(), tss.ITFX.Small(tss.txt, tss.ICX.ML(tss.txt, tss.UI.TextBlock("1.0 \u2013 Poor"), 8))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.UI.Rating(5).SetValue(0).ReadOnly(), tss.ITFX.Small(tss.txt, tss.ICX.ML(tss.txt, tss.UI.TextBlock("Not yet rated"), 8))])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Star Count"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.ICX.W$1(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("3 stars:")), tss.usX.px$1(80)), tss.UI.Rating(3).SetValue(2)]), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.ICX.W$1(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("10 stars:")), tss.usX.px$1(80)), tss.UI.Rating(10).SetValue(7)])])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ResourceCardSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var searchList = new (tss.SearchableList(Tesserae.Tests.Samples.ResourceCardSample.ModelItem)).ctor(this.GetModelItems(), [tss.usX.fr$1(1), tss.usX.fr$1(1), tss.usX.fr$1(1)]).WithNoResultsMessage(function () {
                    return tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock("No models found"));
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ResourceCardSample, 2493, "A card to display a resource"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ResourceCards are used to display a summary of a resource like an AI model, document, or service. They provide optional sections for title, subtitle, tags, description, date, icon, and a footer for commands."), tss.UI.TextBlock("The example below uses a SearchableList in grid mode to render a set of ResourceCards.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [searchList])).SetTitle("Usage")]));
            }
        },
        methods: {
            GetModelItems: function () {
                return System.Array.init([new Tesserae.Tests.Samples.ResourceCardSample.ModelItem("seedream-4.0", "bytedance", "Text-to-Image", "Seedream 4.0 is ByteDance's image creation model combining text-to-image generation and advanced styling capabilities.", "Apr 8, 2026", 3460), new Tesserae.Tests.Samples.ResourceCardSample.ModelItem("nano-banana-2", "google", "Text-to-Image", "Nano Banana 2 is Google's latest image generation model, built on Gemini 3.1 Flash with enhanced efficiency.", "Apr 8, 2026", 3460), new Tesserae.Tests.Samples.ResourceCardSample.ModelItem("veo-3.1", "google", "Text-to-Video", "Veo 3.1 is Google's state-of-the-art video generation model with synchronized native audio generation.", "Apr 8, 2026", 4925), new Tesserae.Tests.Samples.ResourceCardSample.ModelItem("kimi-k2.5", "moonshotai", "Image-Text-to-Text", "Kimi K2.5 is a native multimodal language model from Moonshot AI that understands images and text prompts.", "Apr 8, 2026", 4516), new Tesserae.Tests.Samples.ResourceCardSample.ModelItem("gpt-5.4", "openai", "Text Generation", "GPT-5.4 is OpenAI's most capable frontier model for coding, reasoning, and professional tasks.", "Apr 8, 2026", 1221), new Tesserae.Tests.Samples.ResourceCardSample.ModelItem("claude-opus-4.6", "anthropic", "Text Generation", "Claude Opus 4.6 is Anthropic's flagship language model built for complex, multi-step problem solving.", "Apr 8, 2026", 1221)], Tesserae.Tests.Samples.ResourceCardSample.ModelItem);
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SanitizeHTMLSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var dirtyHtml = "<p>Trusted paragraph <strong>with bold</strong>.</p>\r\n<p><a href=\"https://github.com/curiosity-ai/tesserae\">A safe link</a></p>\r\n<p>Dangerous: <script>alert('xss')</script> inline script.</p>\r\n<img src=x onerror=\"alert('xss')\" />\r\n<iframe src=\"javascript:alert('xss')\"></iframe>\r\n<p onclick=\"alert('xss')\">Click handler attribute.</p>";

                var input = tss.ICX.H(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1(dirtyHtml)), 180);
                var output = tss.ICX.H(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1(tss.SanitizeHTML.Sanitize(dirtyHtml))), 180).ReadOnly();
                var live = tss.UI.MarkdownBlock();
                live.Text = tss.SanitizeHTML.Sanitize(dirtyHtml);

                input.OnInput(function (ta, _) {
                    var sanitized = tss.SanitizeHTML.Sanitize(ta.Text);
                    output.Text = sanitized;
                    live.Text = sanitized;
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SanitizeHTMLSample, 3993, "A utility for sanitizing HTML with DOMPurify"), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("SanitizeHTML is a thin wrapper around the bundled DOMPurify library. Pass it any HTML string and it returns a copy with scripts, dangerous attributes (onerror, onclick, javascript: URLs, ...) and other XSS vectors stripped out."), tss.UI.TextBlock("DOMPurify is bundled with Tesserae and always loaded - there is no preload step, the helper is fully synchronous.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use SanitizeHTML at every trust boundary: whenever you take HTML from an external source (user input, third-party API, AI-generated content, clipboard) and want to drop it into the DOM. Never trust the input upstream of this step. For Markdown sources, prefer MarkdownBlock - it already runs the output through DOMPurify for you.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live demo"), tss.UI.TextBlock("Edit the HTML on the left. The middle pane shows the sanitized string DOMPurify produces; the right pane shows what actually renders. Note that the script tag, onerror handler, javascript: URL and onclick attribute are all dropped."), tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), [tss.ICTX.Children$6(tss.S, tss.ICX.Grow(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Input HTML"), input]), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.UI.VStack()), 16), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Sanitized HTML"), output]), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.UI.VStack()), 16), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rendered"), live])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("One-liner"), tss.UI.TextBlock("Sanitize once, inject anywhere safe:"), tss.UI.MarkdownBlock("```csharp\nvar safe = SanitizeHTML.Sanitize(untrustedHtml);\nsomeElement.innerHTML = safe;\n```")])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SaveButtonSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var saveButton = tss.UI.SaveButton().Pending().OnClickSpinWhile(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
{ }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                });

                var manualButton = tss.UI.SaveButton().NothingToSave();

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SaveButtonSample, 1530, "A button specialized for save operations"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The SaveButton component is a wrapper around a Button that manages common saving states: Pending, Verifying, Saving, Saved, and Error.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [manualButton, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Button$1("Set Nothing to Save").OnClick$1(function () {
                    manualButton.NothingToSave();
                }), tss.UI.Button$1("Set Pending").OnClick$1(function () {
                    manualButton.Pending();
                }), tss.UI.Button$1("Set Verifying").OnClick$1(function () {
                    manualButton.Verifying();
                }), tss.UI.Button$1("Set Saving").OnClick$1(function () {
                    manualButton.Saving();
                }), tss.UI.Button$1("Set Saved").OnClick$1(function () {
                    manualButton.Saved();
                }), tss.UI.Button$1("Set Error").OnClick$1(function () {
                    manualButton.Error("Validation failed!");
                })]).Gap(tss.usX.px$1(8))]).Gap(tss.usX.px$1(16)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live Demo"), tss.UI.TextBlock("Click the button below to simulate a save operation."), saveButton.OnClickSpinWhile(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            saveButton.Verifying();
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(1000)));
                            saveButton.Saving();
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                            saveButton.Saved();
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                            saveButton.NothingToSave();
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                            saveButton.Pending();
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                })])).SetTitle("Manual State Control")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("This SaveButton has a hover text configured. Hover over it when it is in Pending state."), tss.UI.SaveButton().Configure("Disabled", void 0, void 0, void 0, void 0, "Enable Now!", 4598, 4599, true).Pending()])).SetTitle("Hover State")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("This SaveButton text can be updated dynamically."), this.DynamicTextUpdateSample()])).SetTitle("Dynamic Text Update")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("This button verifies using a custom async task."), this.GetVerifyingWhileButton()])).SetTitle("Verifying While")]));
            }
        },
        methods: {
            GetVerifyingWhileButton: function () {
                var btn = tss.UI.SaveButton().Pending();
                btn.OnClickSpinWhile(H5.fn.bind(this, function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            (await H5.toPromise(btn.VerifyingWhile(H5.fn.bind(this, function () {
                                var $tcs = new H5.TCS();
                                (async () => {
                                    {
                                        (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                                        return tss.SaveButton.State.PendingSave;
                                    }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                                return $tcs.task;
                            }))));
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }));
                return btn;
            },
            DynamicTextUpdateSample: function () {
                var btn = tss.UI.SaveButton().Pending();
                return tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [btn, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Button$1("Update Save Text").OnClick$1(function () {
                    btn.Configure("New Save Text", void 0, void 0, void 0, void 0, void 0, 1530, 1530, true);
                }), tss.UI.Button$1("Update Hover Text").OnClick$1(function () {
                    btn.Configure(void 0, void 0, void 0, void 0, void 0, "New Hover Text", 1530, 1530, true);
                })]).Gap(tss.usX.px$1(8))]).Gap(tss.usX.px$1(16));
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SavingToastSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SavingToastSample, 1530, "A toast notification for save operations"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The SavingToast component helps viewing the state of a saving operation (Saving, Saved, Error) with appropriate icons and colors.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1("Trigger Saving").OnClick$1(function () {
                    tss.UI.SavingToast().Saving("Saving data...");
                }), tss.UI.Button$1("Trigger Saved").OnClick$1(function () {
                    tss.UI.SavingToast().Saved("Data saved successfully!");
                }), tss.UI.Button$1("Trigger Error").OnClick$1(function () {
                    tss.UI.SavingToast().Error("Could not save data.");
                }), tss.UI.Button$1("Trigger Error with Close").OnClick$1(function () {
                    tss.UI.SavingToast().Error("Could not save data.", "Error", true);
                }), tss.UI.Button$1("Many Toasts").OnClickSpinWhile(H5.fn.bind(this, function () {
                    return this.ShowMany();
                }))]).Gap(tss.usX.px$1(8)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live Demo"), tss.UI.Button$1("Simulate Save Process").OnClickSpinWhile(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            var savingToast = tss.UI.SavingToast();
                            savingToast.Saving("Starting save...");
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                            savingToast.Saved("All done!");
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            ShowMany: function () {
                var $tcs = new H5.TCS();
                (async () => {
                    {
                        var toast1 = tss.UI.SavingToast();
                        var toast2 = tss.UI.SavingToast();
                        var toast3 = tss.UI.SavingToast();
                        toast1.Saving("Starting save...");
                        toast2.Saving("Starting save...");
                        toast3.Saving("Starting save...");
                        (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                        toast1.Saved("All done!");
                        (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                        toast2.Saved("All done!");
                        (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                        toast3.Saved("All done!");
                    }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                return $tcs.task;
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SearchableGroupedListSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.SearchableGroupedListSample, 3936, "A grouped list that can be searched"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("SearchableGroupedList extends the functionality of SearchableList by adding automatic grouping of items based on a 'Group' property."), tss.UI.TextBlock("It provides a structured way to display filtered results, categorized by logical groups like file types, departments, or priority levels.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use SearchableGroupedList when your dataset has a natural hierarchy or categorization that helps users find items faster. Provide a clear header for each group using the header generator. Ensure that the 'IsMatch' logic considers both the item content and the group name if appropriate. Like SearchableList, provide a meaningful 'No Results' message and use additional command slots for relevant actions.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Grouped Search with Custom Headers"), tss.ICX.MB(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(20), function (s) {
                    return tss.UI.HorizontalSeparator$1(tss.ITFX.SemiBold(tss.txt, tss.txtX.Primary(tss.txt, tss.UI.TextBlock(s)))).Left();
                }).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching records"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Grouped Grid Layout"), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(40), function (s) {
                    return tss.ITFX.Bold(tss.Label, tss.txtX.Primary(tss.Label, tss.UI.Label$1(s)));
                }, [tss.usX.percent$1(33), tss.usX.percent$1(33), tss.usX.percent$1(34)]), tss.usX.px$1(500)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Virtualized Grouped List (10000 items)"), tss.ICX.MB(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(10000), function (s) {
                    return tss.UI.HorizontalSeparator$1(tss.ITFX.SemiBold(tss.txt, tss.txtX.Primary(tss.txt, tss.UI.TextBlock(s)))).Left();
                }).Virtualize(tss.usX.px$1(64)).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching records"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Paginated Grouped List"), tss.ICX.MB(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(50), function (s) {
                    return tss.UI.HorizontalSeparator$1(tss.ITFX.SemiBold(tss.txt, tss.txtX.Primary(tss.txt, tss.UI.TextBlock(s)))).Left();
                }).WithPagination(5).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching records"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (n, i) {
                    return new Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem(System.String.format("Record {0}", [H5.box(n, System.Int32)]), (i % 3 === 0) ? "Category A" : (i % 2 === 0) ? "Category B" : "Category C");
                }).ToArray(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SearchableListSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.SearchableListSample, 3936, "A list that can be searched"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("SearchableList combines a search box with a list of items, providing instant filtering as the user types."), tss.UI.TextBlock("Items must implement the 'ISearchableItem' interface, which defines the matching logic and how each item is rendered.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use SearchableList when you have a moderately sized collection that users need to filter quickly. Ensure the 'IsMatch' implementation is performant and covers all relevant fields. Provide a clear 'No Results' message to help users understand when their search doesn't match anything. Use the 'BeforeSearchBox' and 'AfterSearchBox' slots to add relevant actions like 'Add New' or 'Filter' buttons. For very large datasets, consider server-side filtering or a VirtualizedList.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Searchable List"), tss.ICX.MB(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(10)).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching items found"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Searchable Grid with Commands"), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(24), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)]).BeforeSearchBox([tss.UI.Button$1("Filter").SetIcon$1(1915)]).AfterSearchBox([tss.UI.Button$1("Add Item").Primary().SetIcon$1(3536)]), tss.usX.px$1(400)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Virtualized Searchable List (10000 items)"), tss.ICX.MB(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(10000)).Virtualize(tss.usX.px$1(64)).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching items found"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Paginated Searchable List"), tss.ICX.MB(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(50)).WithPagination(5).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching items found"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (n) {
                    return new Tesserae.Tests.Samples.SearchableListSample.SearchableListItem(System.String.format("Item {0}", [H5.box(n, System.Int32)]));
                }).ToArray(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SearchBoxSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var searchAsYouType = tss.UI.TextBlock("Start typing in the 'Search as you type' box below...");

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SearchBoxSample, 3936, "A control to search"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("SearchBoxes provide an input field for searching through content, allowing users to locate specific items within the website or app."), tss.UI.TextBlock("They include a search icon and a clear button, and support both 'on search' (e.g., when Enter is pressed) and 'search as you type' behaviors.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Always use placeholder text to describe the search scope (e.g., 'Search files'). Use the 'Underlined' style for CommandBars or other minimalist surfaces. Enable 'Search as you type' for small to medium datasets where results can be filtered instantly. Provide a clear visual cue when no results are found. Don't use a SearchBox if you cannot reliably provide accurate results.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic SearchBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Default Search").SetContent(tss.UI.SearchBox("Search...").OnSearch(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Searched for: {0}", [e]));
                })), tss.UI.Label$1("Underlined").SetContent(tss.UI.SearchBox("Search site").Underlined().OnSearch(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Searched for: {0}", [e]));
                })), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.SearchBox("Search disabled").Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Search Behaviors"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Search as you type").SetContent(tss.UI.SearchBox("Type something...").SearchAsYouType().OnSearch(function (s, e) {
                    searchAsYouType.Text = System.String.isNullOrEmpty(e) ? "Waiting for input..." : System.String.format("Current search: {0}", [e]);
                })), searchAsYouType]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Customization"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Custom Icon (Filter)").SetContent(tss.UI.SearchBox("Filter items...").SetIcon(1915)), tss.UI.Label$1("No Icon").SetContent(tss.UI.SearchBox("Iconless search").NoIcon()), tss.UI.Label$1("Fixed Width (250px)").SetContent(tss.ICX.Width(tss.SearchBox, tss.UI.SearchBox("Small search"), tss.usX.px$1(250)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded SearchBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.SearchBox, tss.UI.SearchBox("Small"), tss.BR.Small), tss.ISX.Rounded(tss.SearchBox, tss.UI.SearchBox("Medium"), tss.BR.Medium), tss.ISX.Rounded(tss.SearchBox, tss.UI.SearchBox("Full"), tss.BR.Full)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Keyboard Shortcut"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBlock("Press Ctrl+K (or \u2318K on macOS) anywhere on the page to focus the SearchBox below."), tss.UI.SearchBox("Search...").SetKeyboardShortcut(["Ctrl", "K"])])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SectionStackSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var stack = tss.UI.SectionStack().Secondary();

                this._content = tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SectionStackSample, 151, "A stack with animated sections"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("SectionStack is a high-level layout component designed for creating long-form pages or detailed views. It organizes content into distinct vertical sections, typically with a header and footer, providing a consistent structure for complex information architectures.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use SectionStack for the main content area of your pages. Organize related components into distinct sections to improve readability and scanability. Utilize the 'Title' and 'Commands' features of the SectionStack to provide context and actions at the top of the page.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dynamic Section Generation"), tss.UI.Label$1("Number of sections:").SetContent(tss.UI.Slider(5, 0, 10, 1).OnInput(H5.fn.bind(this, function (s, e) {
                    this.SetChildren(stack, s.Value);
                })))])).SetTitle("Usage")])), stack]);
                this.SetChildren(stack, 5);
            }
        },
        methods: {
            SetChildren: function (stack, count) {
                stack.Clear();
                tss.SectionStackX.Title$1(stack, 589, "Dynamically Generated Sections", "These sections are added based on the slider value", [tss.UI.Button$1("Clear").OnClick$1(function () {
                    stack.Clear();
                })]);

                for (var i = 0; i < count; i = (i + 1) | 0) {
                    tss.SectionStackX.FlatSection(stack, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ITFX.SemiBold(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock(System.String.format("Section {0}", [H5.box(i, System.Int32)])))), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50))]));
                }
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SegmentedPivotSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SegmentedPivotSample, 151, "A segmented navigation pivot"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A SegmentedPivot is a tabbed interface styled as a segmented control. It's best used for toggling between closely related views or filters where space is limited.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Usage"), tss.SegmentedPivotX.SegmentedPivot(tss.SegmentedPivotX.SegmentedPivot(tss.SegmentedPivotX.SegmentedPivot(tss.SegmentedPivotX.SegmentedPivot(tss.UI.SegmentedPivot(), "tab1", tss.UI.SegmentTitle("Overview"), function () {
                    return tss.UI.CenteredWithBackground(tss.UI.Message("Overview Content"));
                }), "tab2", tss.UI.SegmentTitle("Logs"), function () {
                    return tss.UI.CenteredWithBackground(tss.UI.Message("Logs Content"));
                }), "tab3", tss.UI.SegmentTitle("Analytics"), function () {
                    return tss.UI.CenteredWithBackground(tss.UI.Message("Analytics Content"));
                }), "tab4", tss.UI.SegmentTitle("Firewall"), function () {
                    return tss.UI.CenteredWithBackground(tss.UI.Message("Firewall Content"));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SidebarSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                CreateDeepNav: function (path, currentDepth, maxDepth) {
                    return new (H5.GeneratorEnumerable$1(Tesserae.ISidebarItem))(H5.fn.bind(this, function (path, currentDepth, maxDepth) {
                        var $s = 0,
                            $jff,
                            $rv,
                            HandleChange,
                            $ae;

                        var $en = new (H5.GeneratorEnumerator$1(Tesserae.ISidebarItem))(H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    switch ($s) {
                                        case 0: {
                                            if (currentDepth === void 0) { currentDepth = 0; }
                                                if (maxDepth === void 0) { maxDepth = 3; }
                                                if (currentDepth < maxDepth) {
                                                    $s = 1;
                                                    continue;
                                                } 
                                                $s = 5;
                                                continue;
                                        }
                                        case 1: {
                                            HandleChange = function (e) {
                                                    tss.UI.Dialog(System.String.format("Move element {0} from {1} to {2}?", e.Item.OwnIdentifier, e.From.OwnIdentifier, e.To.OwnIdentifier)).YesNo(void 0, e.Cancel, void 0, void 0);
                                                };
                                                $en.current = new Tesserae.SidebarNav.$ctor1(System.String.format("{0}/{1}.1", path, H5.box(((currentDepth + 1) | 0), System.Int32)), 351, System.String.format("{0}/{1}.1", path, H5.box(((currentDepth + 1) | 0), System.Int32)), true).Sortable(true, "trees").AddRange(Tesserae.Tests.Samples.SidebarSample.CreateDeepNav(System.String.format("{0}/{1}.1", path, H5.box(((currentDepth + 1) | 0), System.Int32)), ((currentDepth + 1) | 0), maxDepth)).OnParentChanged(HandleChange);
                                                $s = 2;
                                                return true;
                                        }
                                        case 2: {
                                            $en.current = new Tesserae.SidebarNav.$ctor1(System.String.format("{0}/{1}.2", path, H5.box(((currentDepth + 1) | 0), System.Int32)), 351, System.String.format("{0}/{1}.2", path, H5.box(((currentDepth + 1) | 0), System.Int32)), true).Sortable(true, "trees").AddRange(Tesserae.Tests.Samples.SidebarSample.CreateDeepNav(System.String.format("{0}/{1}.2", path, H5.box(((currentDepth + 1) | 0), System.Int32)), ((currentDepth + 1) | 0), maxDepth)).OnParentChanged(HandleChange);
                                                $s = 3;
                                                return true;
                                        }
                                        case 3: {
                                            $en.current = new Tesserae.SidebarNav.$ctor1(System.String.format("{0}/{1}.3", path, H5.box(((currentDepth + 1) | 0), System.Int32)), 351, System.String.format("{0}/{1}.3", path, H5.box(((currentDepth + 1) | 0), System.Int32)), true).Sortable(true, "trees").AddRange(Tesserae.Tests.Samples.SidebarSample.CreateDeepNav(System.String.format("{0}/{1}.3", path, H5.box(((currentDepth + 1) | 0), System.Int32)), ((currentDepth + 1) | 0), maxDepth)).OnParentChanged(HandleChange);
                                                $s = 4;
                                                return true;
                                        }
                                        case 4: {
                                            $s = 5;
                                            continue;
                                        }
                                        case 5: {

                                        }
                                        default: {
                                            return false;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                throw $ae;
                            }
                        }));
                        return $en;
                    }, arguments));
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var sidebar = tss.UI.Sidebar();

                var searchBox = new Tesserae.SidebarSearchBox("search", "Search Sidebar...");
                searchBox.OnSearch(function (term) {
                    sidebar.Search(term);
                });

                sidebar.AddHeader(searchBox);

                sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("home", 2402, "Home"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("profile", 4799, "Profile"));

                sidebar.AddContent(new Tesserae.SidebarSeparator("sep1", "Grouping"));

                var tabs = new tss.SidebarSegmentedPivot("tabs").Add("tab1", tss.UI.SegmentTitle$1("Tab 1", 3802), [new Tesserae.SidebarButton.$ctor14("t1_btn1", 3802, "Launch"), new Tesserae.SidebarButton.$ctor14("t1_btn2", 3802, "Launch 2")]).Add("tab2", tss.UI.SegmentTitle$1("Tab 2", 3491), [new Tesserae.SidebarButton.$ctor14("t2_btn1", 2140, "World"), new Tesserae.SidebarButton.$ctor14("t2_btn2", 2140, "World 2")]);

                sidebar.AddContent(tabs);

                var settingsNav = new Tesserae.SidebarNav.$ctor2("settings", 3974, "Settings", true);


                settingsNav.Add(new Tesserae.SidebarButton.$ctor14("general", 3974, "General"));
                settingsNav.Add(new Tesserae.SidebarButton.$ctor14("security", 2802, "Security"));
                settingsNav.Add(new Tesserae.SidebarButton.$ctor14("privacy", 1749, "Privacy"));

                sidebar.AddContent(settingsNav);

                sidebar.AddContent(new Tesserae.SidebarSeparator("sep2"));

                sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("help", 3654, "Help"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor6("link", "https://bing.com", 2761, "External Link"));


                var lightDark = new Tesserae.SidebarCommand.$ctor7(4391).Tooltip$1("Light Mode");

                lightDark.OnClick(function () {
                    if (tss.UI.Theme.IsDark) {
                        tss.UI.Theme.Light();
                        lightDark.SetIcon$1(4391).Tooltip$1("Light Mode");
                    } else {
                        tss.UI.Theme.Dark();
                        lightDark.SetIcon$1(3053).Tooltip$1("Dark Mode");
                    }
                });

                var toast = new Tesserae.SidebarCommand.$ctor5(450).Tooltip$1("Toast !").OnClick(function () {
                    tss.UI.Toast().Success("Here is your toast \ud83c\udf5e");
                });
                var pizza = new Tesserae.SidebarCommand.$ctor5(468).Tooltip$1("Pizza!").OnClick(function () {
                    tss.UI.Toast().Success("Here is your pizza \ud83c\udf55");
                });
                var cheese = new Tesserae.SidebarCommand.$ctor5(454).Tooltip$1("Cheese !").OnClick(function () {
                    tss.UI.Toast().Success("Here is your cheese \ud83e\uddc0");
                });

                var commands = new Tesserae.SidebarCommands("TOASTS", [lightDark, toast, pizza, cheese]);


                var fireworks = new Tesserae.SidebarCommand.$ctor5(838).Tooltip$1("Confetti !").OnClick(function () {
                    tss.UI.Toast().Success("\ud83c\udf8a");
                });
                var happy = new Tesserae.SidebarCommand.$ctor5(9).Tooltip$1("I like this !").OnClick(function () {
                    tss.UI.Toast().Success("Thanks for your feedback");
                });
                var sad = new Tesserae.SidebarCommand.$ctor5(51).Tooltip$1("I don't like this!").OnClick(function () {
                    tss.UI.Toast().Success("Thanks for your feedback");
                });

                var dotsMenu = new Tesserae.SidebarCommand.$ctor7(2944).OnClickMenu(function () {
                    return System.Array.init([new Tesserae.SidebarButton.$ctor14("MANAGE_ACCOUNT", 4799, "Manage Account"), new Tesserae.SidebarButton.$ctor14("PREFERENCES", 3974, "Preferences"), new Tesserae.SidebarButton.$ctor14("DELETE", 4675, "Delete Account"), new Tesserae.SidebarCommands("EMOTIONS", [new Tesserae.SidebarCommand.$ctor5(9), new Tesserae.SidebarCommand.$ctor5(51), new Tesserae.SidebarCommand.$ctor5(53)]), new Tesserae.SidebarCommands("ADD_DELETE", [new Tesserae.SidebarCommand.$ctor7(3536).Primary(), new Tesserae.SidebarCommand.$ctor7(4675).Danger()]).AlignEnd(), new Tesserae.SidebarButton.$ctor14("SIGNOUT", 4051, "Sign Out")], Tesserae.ISidebarItem);
                });

                var commandsEndAligned = new Tesserae.SidebarCommands("SETTINGS", [fireworks, dotsMenu]).AlignEnd();

                sidebar.AddFooter(new Tesserae.SidebarNav.$ctor1("DEEP_NAV", 350, "Multi-Depth Nav", true).Sortable(true, "trees").AddRange(Tesserae.Tests.Samples.SidebarSample.CreateDeepNav("root")));

                sidebar.AddFooter(new Tesserae.SidebarNav.$ctor1("EMPTY_NAV", 853, "Empty Nav", true).OnOpenIconClick$1(function (e, m) {
                    tss.UI.Toast().Success("You clicked on the icon!");
                }));


                sidebar.AddFooter(commands);
                sidebar.AddFooter(commandsEndAligned);

                sidebar.AddFooter(new Tesserae.SidebarButton.$ctor3("CURIOSITY_REF", "https://curiosity.ai", new tss.ImageIcon("/assets/img/curiosity-logo.svg"), "By Curiosity", new Tesserae.SidebarBadge.ctor("+3").Foreground(tss.UI.Theme.Primary.Foreground).Background(tss.UI.Theme.Primary.Background), [new Tesserae.SidebarCommand.$ctor3("https://github.com/curiosity-ai/tesserae", 237)]).Tooltip$1("Made with \u2764 by Curiosity"));


                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SidebarSample, 151, "A sidebar navigation component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A fully featured Sidebar with Search, Navigation, Buttons, and Separators.")])).SetTitle("Overview"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.H(tss.SplitView, tss.ICX.WS(tss.SplitView, tss.UI.SplitView()), 800).LeftIsSmaller(tss.usX.px$1(400)).Resizable().Left(tss.ICX.S(tss.Sidebar, sidebar)).Right(tss.UI.CenteredCardWithBackground(tss.UI.Message("Your application content goes here")))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SidebarSeparatorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var sidebar = tss.UI.Sidebar();

                sidebar.AddHeader(new Tesserae.SidebarText("header", "Header"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("1", 2402, "Home"));
                sidebar.AddContent(new Tesserae.SidebarSeparator("sep1"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("2", 4799, "Profile"));
                sidebar.AddContent(new Tesserae.SidebarSeparator("sep2", "More Options"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("3", 3974, "Settings"));

                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SidebarSeparatorSample, 2991, "A visual separator for sidebar items"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A separator for the Sidebar component to visually group items.")])).SetTitle("Overview"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Basic separator:"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.H$1(tss.Sidebar, tss.ICX.S(tss.Sidebar, sidebar), tss.usX.px$1(500))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SidenavSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                SwitchTo: function (sidenav, sidebar, title, identifier, sectionName, fill) {
                    sidenav.Select(identifier);
                    title.SetText(sectionName);
                    sidebar.ClearContent();
                    fill(sidebar);
                },
                FillHomeSection: function (sidebar) {
                    sidebar.AddContent(new Tesserae.SidebarSeparator("home-sep", "Home"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("home-overview", 151, "Overview").Selected());
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("home-news", 411, "Notifications"));
                },
                FillOperateSection: function (sidebar) {
                    sidebar.AddContent(new Tesserae.SidebarSeparator("op-sep", "Operate"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("op-monitoring", 3628, "Monitoring").Selected());
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("op-alerts", 411, "Alerts"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("op-logs", 2773, "Logs"));
                },
                FillBuildSection: function (sidebar) {
                    sidebar.AddContent(new Tesserae.SidebarSeparator("build-sep", "Build"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("data-sources", 1424, "Data Sources").Selected());
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("graph-db", 1468, "Graph DB"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("search-cfg", 3936, "Search config"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("ai-studio", 4324, "AI Studio"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("nlp-studio", 1663, "NLP Studio"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("endpoints", 2761, "Endpoints"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("integrations", 3522, "Integrations"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("interface", 686, "Interface"));
                },
                FillGovernSection: function (sidebar) {
                    sidebar.AddContent(new Tesserae.SidebarSeparator("gov-sep", "Govern"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("gov-policies", 3990, "Policies").Selected());
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("gov-audit", 3936, "Audit log"));
                },
                FillConfigureSection: function (sidebar) {
                    sidebar.AddContent(new Tesserae.SidebarSeparator("cfg-sep", "Configure"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("cfg-general", 3974, "General").Selected());
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("cfg-users", 4799, "Users"));
                    sidebar.AddContent(new Tesserae.SidebarButton.$ctor14("cfg-billing", 4324, "Billing"));
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var sectionTitle = new Tesserae.SidebarText("title", "Build", void 0, "tss-fontsize-medium", "tss-fontweight-semibold");

                var sidebar = tss.UI.Sidebar();
                sidebar.AddHeader(sectionTitle);

                Tesserae.Tests.Samples.SidenavSample.FillBuildSection(sidebar);

                var sidenav = tss.UI.Sidenav();

                sidenav.AddHeader(new Tesserae.SidenavButton.$ctor2("brand", 3802, "App").AsBrand().Tooltip("My App"));

                var home = new Tesserae.SidenavButton.$ctor2("home", 2402, "Home");
                var operate = new Tesserae.SidenavButton.$ctor2("operate", 3628, "Operate").ShowDot().DotDanger();
                var build = new Tesserae.SidenavButton.$ctor2("build", 1424, "Build").Selected();
                var govern = new Tesserae.SidenavButton.$ctor2("govern", 3990, "Govern");
                var configure = new Tesserae.SidenavButton.$ctor2("configure", 3974, "Configure");

                sidenav.AddContent(home);
                sidenav.AddContent(operate);
                sidenav.AddContent(build);
                sidenav.AddContent(govern);
                sidenav.AddContent(configure);

                home.OnClick(function () {
                    Tesserae.Tests.Samples.SidenavSample.SwitchTo(sidenav, sidebar, sectionTitle, "home", "Home", Tesserae.Tests.Samples.SidenavSample.FillHomeSection);
                });
                operate.OnClick(function () {
                    Tesserae.Tests.Samples.SidenavSample.SwitchTo(sidenav, sidebar, sectionTitle, "operate", "Operate", Tesserae.Tests.Samples.SidenavSample.FillOperateSection);
                });
                build.OnClick(function () {
                    Tesserae.Tests.Samples.SidenavSample.SwitchTo(sidenav, sidebar, sectionTitle, "build", "Build", Tesserae.Tests.Samples.SidenavSample.FillBuildSection);
                });
                govern.OnClick(function () {
                    Tesserae.Tests.Samples.SidenavSample.SwitchTo(sidenav, sidebar, sectionTitle, "govern", "Govern", Tesserae.Tests.Samples.SidenavSample.FillGovernSection);
                });
                configure.OnClick(function () {
                    Tesserae.Tests.Samples.SidenavSample.SwitchTo(sidenav, sidebar, sectionTitle, "configure", "Configure", Tesserae.Tests.Samples.SidenavSample.FillConfigureSection);
                });

                sidenav.AddFooter(new Tesserae.SidenavButton.$ctor2("user", 4799, "Account").Tooltip("OA \u2014 Account"));

                var standaloneSidenav = tss.UI.Sidenav();
                standaloneSidenav.AddHeader(new Tesserae.SidenavButton.$ctor2("brand2", 3802, "App").AsBrand().Tooltip("My App"));
                standaloneSidenav.AddContent(new Tesserae.SidenavButton.$ctor2("dash", 1411, "Dashboard").Selected());
                standaloneSidenav.AddContent(new Tesserae.SidenavButton.$ctor2("docs", 1551, "Docs"));
                standaloneSidenav.AddContent(new Tesserae.SidenavButton.$ctor2("chart", 929, "Charts"));
                standaloneSidenav.AddContent(new Tesserae.SidenavButton.$ctor2("inbox", 1702, "Inbox").ShowDot());
                standaloneSidenav.AddFooter(new Tesserae.SidenavButton.$ctor2("settings", 3974, "Settings"));
                standaloneSidenav.AddFooter(new Tesserae.SidenavButton.$ctor2("user2", 4799, "User"));

                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SidenavSample, 151, "A vertical icon-only navigation rail"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A Sidenav is a narrow, vertical icon navigation rail intended to be used as the leftmost navigation in an application. Each item shows an icon with a small label below it. The Sidenav can be combined with a Sidebar to its right to create a two-level navigation experience.")])).SetTitle("Overview"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("The Sidenav (left rail) selects the high-level section, and the Sidebar shows context for that section."), tss.usX.px$1(8)), tss.ICTX.Children$6(tss.S, tss.ICX.H(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), 600), [tss.ICX.HS(tss.Sidenav, sidenav), tss.ICX.HS(tss.Sidebar, sidebar), tss.ICTX.Children$6(tss.S, tss.ICX.Padding(tss.S, tss.ICX.HS(tss.S, tss.ICX.Grow(tss.S, tss.UI.VStack())), tss.usX.px$1(16)), [tss.UI.Message("Application content goes here")])])])).SetTitle("Sidenav + Sidebar"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("A Sidenav can also be used standalone as the only navigation in a smaller app."), tss.usX.px$1(8)), tss.ICTX.Children$6(tss.S, tss.ICX.H(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), 400), [tss.ICX.HS(tss.Sidenav, standaloneSidenav), tss.ICTX.Children$6(tss.S, tss.ICX.Padding(tss.S, tss.ICX.HS(tss.S, tss.ICX.Grow(tss.S, tss.UI.VStack())), tss.usX.px$1(16)), [tss.UI.Message("Application content goes here")])])])).SetTitle("Standalone")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SkeletonSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SkeletonSample, 568, "A placeholder component for loading state"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Skeleton loaders are used to provide a placeholder for content that is still loading. They help reduce the perceived load time and prevent layout shifts by reserving the space that the final content will occupy."), tss.UI.TextBlock("They come in various shapes like circles for avatars and rectangles for text or images.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use skeleton loaders when content takes more than a second to load. Match the shape and size of the skeleton as closely as possible to the actual content it replaces. Avoid using skeletons for very fast-loading content as it can cause flickering. Ensure the skeleton's color and animation are subtle and fit with the overall theme.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Avatar and Text Placeholder"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.H(tss.Skeleton, tss.ICX.W(tss.Skeleton, tss.UI.Skeleton(Tesserae.SkeletonType.Circle), 48), 48), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.MB(tss.Skeleton, tss.ICX.ML(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W(tss.Skeleton, tss.UI.Skeleton(), 200), 16), 16), 8), tss.ICX.ML(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W(tss.Skeleton, tss.UI.Skeleton(), 140), 12), 16)])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Article/Image Placeholder"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.H(tss.Skeleton, tss.ICX.WS(tss.Skeleton, tss.UI.Skeleton(Tesserae.SkeletonType.Rect)), 200), tss.ICX.MT(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.WS(tss.Skeleton, tss.UI.Skeleton()), 16), 16), tss.ICX.MT(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W$1(tss.Skeleton, tss.UI.Skeleton(), tss.usX.percent$1(80)), 16), 8), tss.ICX.MT(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W$1(tss.Skeleton, tss.UI.Skeleton(), tss.usX.percent$1(60)), 16), 8)])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SliderSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var value = new (tss.SettableObservableT(System.Int32))(50);
                var s1 = tss.UI.Slider(50, 0, 100, 1).OnInput(function (s, e) {
                    value.Value$1 = s.Value;
                });
                var s2 = tss.UI.Slider(20, 0, 100, 10).OnInput(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Value changed to {0}", [H5.box(s.Value, System.Int32)]));
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SliderSample, 3975, "A control to select a value from a range"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Sliders allow users to select a value from a continuous or discrete range of values by moving a thumb along a track."), tss.UI.TextBlock("They are ideal for settings that don't require high precision but benefit from a visual representation of the available range, such as volume or brightness.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use sliders when users need to choose a value from a range where the relative position is more important than the exact value. Provide clear labels for the minimum and maximum values. If the user needs to select a precise number, consider using a NumberPicker alongside or instead of a slider. Use discrete steps (increments) if the available choices are limited to specific intervals.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Sliders"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Continuous Slider (step: 1)").SetContent(s1), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("Current Value: "), tss.UI.DeferSync$3(System.Int32, value, function (v) {
                    return tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(H5.toString(v)));
                })]), tss.UI.Label$1("Discrete Slider (step: 10)").SetContent(s2)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("States"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Slider")).SetContent(tss.UI.Slider(30, 0, 100, 10).Disabled()), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required Slider")).SetContent(tss.UI.Slider(10, 0, 100, 10))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Vertical Sliders"), tss.ICTX.Children$6(tss.S, tss.ICX.H$1(tss.S, tss.UI.HStack(), tss.usX.px$1(150)).AlignItems("center"), [tss.ICTX.Children$6(tss.S, tss.ICX.H$1(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack().AlignItems("center")), tss.usX.px$1(150)), [tss.ICX.HS(tss.Slider, tss.UI.Slider(50, 0, 100, 10).Vertical()), tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("50%")), 4)]), tss.ICTX.Children$6(tss.S, tss.ICX.H$1(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack().AlignItems("center")), tss.usX.px$1(150)), [tss.ICX.HS(tss.Slider, tss.UI.Slider(20, 0, 100, 10).Vertical()), tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("20%")), 4)]), tss.ICTX.Children$6(tss.S, tss.ICX.H$1(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack().AlignItems("center")), tss.usX.px$1(150)), [tss.ICX.HS(tss.Slider, tss.UI.Slider(80, 0, 100, 10).Vertical()), tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("80%")), 4)]), tss.ICTX.Children$6(tss.S, tss.ICX.H$1(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack().AlignItems("center")), tss.usX.px$1(150)), [tss.ICX.HS(tss.Slider, tss.UI.Slider(50, 0, 100, 10).Vertical()).Disabled(), tss.ICX.MT(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Disabled")), 4)])])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SparklineSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SparklineSample, 931, "A compact inline trend chart"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Sparkline renders a compact SVG area chart designed to be embedded alongside metric values, table cells, or dashboard cards. It conveys the shape of a trend at a glance without axes, labels, or interactive controls."), tss.UI.TextBlock("The chart accepts a data array of doubles and optional width, height, and color parameters, making it easy to match any layout or brand color.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Pair a Sparkline with a headline metric value so users can interpret the trend in context. Keep the data array between 10 and 50 points \u2014 fewer points produce sharp angular charts, while more than 50 can add unnecessary rendering overhead in dense dashboards. Choose colors that contrast with the card background; use semantic colors (green for growth, red for decline) when the trend direction carries meaning.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Upward Trend"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Revenue"), tss.usX.px$1(120)), tss.UI.Sparkline(System.Array.init([10, 14, 18, 15, 22, 28, 25, 35, 40, 50], System.Double), 150, 40, tss.UI.Theme.Colors.Green600)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Downward Trend"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Error Rate"), tss.usX.px$1(120)), tss.UI.Sparkline(System.Array.init([50, 45, 40, 48, 30, 28, 20, 15, 12, 10], System.Double), 150, 40, "var(--tss-danger-background-color)")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Flat / Stable"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Latency (ms)"), tss.usX.px$1(120)), tss.UI.Sparkline(System.Array.init([30, 31, 29, 30, 32, 30, 31, 29, 30, 31], System.Double), 150, 40, tss.UI.Theme.Colors.Neutral500)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Volatile / Random"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Requests"), tss.usX.px$1(120)), tss.UI.Sparkline(System.Array.init([10, 40, 15, 55, 20, 60, 5, 45, 30, 70, 25, 50], System.Double), 150, 40, tss.UI.Theme.Colors.Blue600)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Primary Color (default)"), tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.ICX.W$1(tss.txt, tss.UI.TextBlock("Conversions"), tss.usX.px$1(120)), tss.UI.Sparkline(System.Array.init([10, 20, 15, 30, 25, 40, 35, 50], System.Double), 150, 40, "")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Larger Chart"), tss.ICX.WS(tss.Sparkline, tss.UI.Sparkline(System.Array.init([5, 10, 8, 20, 18, 35, 30, 45, 42, 60, 55, 70], System.Double), 300, 80, tss.UI.Theme.Colors.Green600))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SpinnerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.SpinnerSample, 4226, "A spinner component"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Spinners are animated circular indicators used to show that a task is in progress when the exact duration is unknown. They are subtle, lightweight, and can be easily placed inline with content or centered within a container to provide feedback without disrupting the layout.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use a Spinner for tasks that take more than a second but have an indeterminate end time. Include a brief, descriptive label (e.g., 'Loading...', 'Processing...') to give users context. Choose a size that is appropriate for the surrounding content\u2014smaller for inline elements and larger for full-page loading states. Avoid showing multiple spinners simultaneously if possible.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner sizes")), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Extra small spinner").SetContent(tss.UI.Spinner().XSmall())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Small spinner").SetContent(tss.UI.Spinner().Small())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Medium spinner").SetContent(tss.UI.Spinner().Medium())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Large spinner").SetContent(tss.UI.Spinner().Large()))])).SetTitle("Usage")])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner label positioning")), tss.UI.Label$1("Spinner with label positioned below").SetContent(tss.UI.Spinner("I am definitely loading...").Below()), tss.UI.Label$1("Spinner with label positioned above").SetContent(tss.UI.Spinner("Seriously, still loading...").Above()), tss.UI.Label$1("Spinner with label positioned to right").SetContent(tss.UI.Spinner("Wait, wait...").Right()), tss.UI.Label$1("Spinner with label positioned to left").SetContent(tss.UI.Spinner("Nope, still loading...").Left())])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner with fixed progress")), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("25% Progress").SetContent(tss.UI.Spinner().Progress$1(25).Large())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("50% Progress").SetContent(tss.UI.Spinner().Progress$1(50).Large())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("75% Progress").SetContent(tss.UI.Spinner().Progress$1(75).Large())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("100% Progress").SetContent(tss.UI.Spinner().Progress$1(100).Large()))]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SplitViewSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var splitView = tss.ICX.H(tss.SplitView, tss.ICX.WS(tss.SplitView, tss.UI.SplitView().Left(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral100), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Left Panel")))])).Right(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral200), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Right Panel")))])).Resizable()), 200);
                var horzSplitView = tss.ICX.H(tss.HorizontalSplitView, tss.ICX.WS(tss.HorizontalSplitView, tss.UI.HorizontalSplitView().Top(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral100), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Top Panel")))])).Bottom(tss.ICTX.Children$6(tss.S, tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), tss.UI.Theme.Colors.Neutral200), [tss.ICX.AlignCenter(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Bottom Panel")))])).Resizable()), 200);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.SplitViewSample, 151, "A component to split the view"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("SplitViews divide a surface into two areas, either horizontally or vertically. They are commonly used for master-detail layouts, navigation sidebars, or resizable workspace areas."), tss.UI.TextBlock("Tesserae provides both 'SplitView' (vertical split) and 'HorizontalSplitView' with support for resizable handles and initial sizing.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use SplitViews when users need to see two related sets of content at the same time. Enable resizability if the ideal balance between the two panels depends on the user's task or screen size. Set sensible minimum and maximum sizes for the panels to prevent them from disappearing or becoming too large. Use distinct background colors or borders to help users distinguish between the two areas.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interaction Controls"), tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), [tss.UI.Button$1("Make Non-Resizable").OnClick$1(function () {
                    splitView.NotResizable();
                    horzSplitView.NotResizable();
                    tss.UI.Toast().Information("Resizing disabled");
                }), tss.UI.Button$1("Make Resizable").Primary().OnClick$1(function () {
                    splitView.Resizable();
                    horzSplitView.Resizable();
                    tss.UI.Toast().Information("Resizing enabled");
                })]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Vertical SplitView"), tss.ICX.MB(tss.SplitView, splitView, 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Horizontal SplitView"), horzSplitView])).SetTitle("Usage")]), true, false, "");
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.StackSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            fields: {
                sampleSortableStackLocalStorageKey: null
            },
            ctors: {
                init: function () {
                    this.sampleSortableStackLocalStorageKey = "sampleSortableStackOrder";
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var mainButton = tss.ICX.MinWidth(tss.Button, tss.ITFX.TextLeft(tss.Button, tss.UI.Button$1("Hover me")), tss.usX.px$1(200));
                var otherButton = tss.ICX.Fade$2(tss.Button, tss.UI.Button$1().SetIcon$1(4546, tss.UI.Theme.Danger.Background, "tss-fontsize-small", "fi-rr-", false));
                var hoverStack = tss.ICTX.Children$6(tss.S, tss.ICX.MaxWidth(tss.S, tss.UI.HStack(), tss.usX.px$1(500)), [mainButton, otherButton]);

                var sortableStack = tss.ICX.MaxWidth(Tesserae.SortableStack, tss.ICX.PB(Tesserae.SortableStack, tss.ICX.WS(Tesserae.SortableStack, new Tesserae.SortableStack(tss.S.Orientation.Horizontal)).AlignItemsCenter(), 8), tss.usX.px$1(500));
                sortableStack.Add("1", tss.UI.Button$1().SetIcon$1(3));
                sortableStack.Add("2", tss.UI.Button$1().SetIcon$1(4));
                sortableStack.Add("3", tss.UI.Button$1().SetIcon$1(5));
                sortableStack.Add("4", tss.UI.Button$1().SetIcon$1(14));
                sortableStack.Add("5", tss.UI.Button$1().SetIcon$1(16));

                var stack = tss.UI.Stack();
                var countSlider = tss.UI.Slider(5, 0, 10, 1);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.StackSample, 151, "A layout container that stacks its children"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Stacks are container components that simplify the use of Flexbox for layout. They allow you to arrange children components either horizontally (HStack) or vertically (VStack)."), tss.UI.TextBlock("Tesserae's Stack also includes advanced features like 'SortableStack' for drag-and-drop reordering.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Stacks as the primary way to organize your UI elements. Use HStack for side-by-side components and VStack for top-to-bottom arrangements. Leverage the 'Gap' property to ensure consistent spacing between children. Use SortableStack when users need to customize the order of items, such as in a dashboard or task list. Avoid deeply nested stacks if a Grid layout would be more appropriate for the complexity.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live Layout Playground"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Number of items:").SetContent(countSlider.OnInput(H5.fn.bind(this, function (s, e) {
                    this.SetChildren(stack, s.Value);
                }))), tss.UI.ChoiceGroup("Orientation:").Horizontal().Choices([tss.UI.Choice("Vertical").Selected(), tss.UI.Choice("Horizontal"), tss.UI.Choice("Vertical Reverse"), tss.UI.Choice("Horizontal Reverse")]).OnChange(function (s, e) {
                    if (H5.rE(s.SelectedOption.Text, "Horizontal")) {
                        stack.Horizontal();
                    } else {
                        if (H5.rE(s.SelectedOption.Text, "Vertical")) {
                            stack.Vertical();
                        } else {
                            if (H5.rE(s.SelectedOption.Text, "Horizontal Reverse")) {
                                stack.HorizontalReverse();
                            } else {
                                if (H5.rE(s.SelectedOption.Text, "Vertical Reverse")) {
                                    stack.VerticalReverse();
                                }
                            }
                        }
                    }
                })])]), 16), tss.UI.Card(tss.ICX.HeightAuto(tss.S, stack))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Sortable Stack"), tss.UI.TextBlock("Drag and drop these buttons to reorder them. The state is saved to local storage."), tss.ICX.MB(Tesserae.SortableStack, sortableStack, 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Events"), tss.UI.TextBlock("Stacks can respond to mouse events, allowing for complex hover behaviors."), hoverStack.OnMouseOver(function (s, e) {
                    tss.ICX.Show(tss.Button, otherButton);
                }).OnMouseOut(function (s, e) {
                    tss.ICX.Fade$2(tss.Button, otherButton);
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Stacks"), tss.UI.TextBlock("Rounding is visible when the stack has a background or border."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.W(tss.S, tss.ISX.Background(tss.S, tss.ISX.Rounded(tss.S, tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBlock("Small")]), 16), tss.BR.Small), tss.UI.Theme.Colors.Blue200), 100).AlignItemsCenter(), tss.ICX.W(tss.S, tss.ISX.Background(tss.S, tss.ISX.Rounded(tss.S, tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBlock("Medium")]), 16), tss.BR.Medium), tss.UI.Theme.Colors.Blue200), 100).AlignItemsCenter(), tss.ICX.W(tss.S, tss.ISX.Background(tss.S, tss.ISX.Rounded(tss.S, tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBlock("Full")]), 16), tss.BR.Full), tss.UI.Theme.Colors.Blue200), 100).AlignItemsCenter()])])).SetTitle("Usage")]));
                this.SetChildren(stack, 5);
            }
        },
        methods: {
            SetChildren: function (stack, count) {
                stack.Clear();
                for (var i = 0; i < count; i = (i + 1) | 0) {
                    stack.Add(tss.UI.Button$1(System.String.format("Item {0}", [H5.box(i, System.Int32)])));
                }
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.StepperSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.StepperSample, 4016, "A component to display a step-by-step process"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Steppers (also known as Wizards) guide users through a multi-step process by breaking it down into smaller, logical chunks."), tss.UI.TextBlock("They manage the visibility of content for each step and provide built-in navigation controls (Previous/Next) while tracking the current progress.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Steppers for complex tasks that have a clear sequential order. Keep each step focused on a single topic to avoid overwhelming the user. Provide clear labels for each step so the user knows what to expect. Use the 'Review' step to allow users to verify their input before the final submission. Ensure that the 'Previous' action allows users to return and modify their entries without losing data.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Registration Wizard"), tss.UI.Stepper([tss.UI.Step("Personal Info", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.MB(tss.txt, tss.UI.TextBlock("Tell us about yourself:"), 16), tss.UI.Label$1("Full Name").SetContent(tss.UI.TextBox$1().SetPlaceholder("John Doe")), tss.UI.Label$1("Email Address").SetContent(tss.UI.TextBox$1().SetPlaceholder("john@example.com"))])), tss.UI.Step("Preferences", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.MB(tss.txt, tss.UI.TextBlock("Customize your experience:"), 16), tss.UI.Toggle$3(tss.UI.TextBlock("Yes"), tss.UI.TextBlock("No")).Checked(), tss.UI.Toggle$3(tss.UI.TextBlock("Dark"), tss.UI.TextBlock("Light")), tss.UI.Label$1("Favorite Color").SetContent(tss.UI.ColorPicker())])), tss.UI.Step("Terms & Review", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.MB(tss.txt, tss.UI.TextBlock("Please review and accept our terms:"), 16), tss.UI.Card(tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Detailed terms and conditions text goes here..."))), tss.txtX.Required(tss.Label, tss.UI.Label$1("Acceptance")).SetContent(tss.UI.CheckBox$1("I agree to the terms of service")), tss.ICX.MT(tss.Button, tss.UI.Button$1("Complete Registration").Primary(), 16)]))]).OnStepChange(function (s) {
                    tss.UI.Toast().Information(System.String.format("Step {0}: {1}", H5.box(((s.CurrentStepIndex + 1) | 0), System.Int32), s.CurrentStep.Title));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.StepsSliderSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var sizeValue = new (tss.SettableObservableT(System.String))("M");
                var percentValue = new (tss.SettableObservableT(System.Int32))(50);

                var sizeSlider = new (tss.StepsSlider(System.String))(["XS", "S", "M", "L", "XL"]).OnChange(function (v) {
                    sizeValue.Value$1 = v;
                    tss.UI.Toast().Information(System.String.format("Size changed to: {0}", [v]));
                });

                var percentSlider = new (tss.StepsSlider(System.Int32))([0, 25, 50, 75, 100]).OnChange(function (v) {
                    percentValue.Value$1 = v;
                });

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.StepsSliderSample, 3975, "A slider that snaps to discrete named steps"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("StepsSlider is a generic slider component that constrains movement to a fixed set of named values. Unlike a continuous slider, it only ever lands on one of the provided steps, making it suitable for cases where only a handful of distinct choices are valid."), tss.UI.TextBlock("Common uses include selecting T-shirt sizes, quality levels, priority tiers, or any domain-specific ordered category where a free numeric value would be meaningless.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use StepsSlider when the number of valid values is small (typically 3\u20137) and each value has a clear, user-facing name. Prefer a Dropdown or ChoiceGroup when there are many options or the labels are long. Always show the currently selected value adjacent to the slider so users understand what they have chosen.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("String Steps (T-shirt Sizes)"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Size").SetContent(sizeSlider), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("Selected: "), tss.UI.DeferSync$3(System.String, sizeValue, function (v) {
                    return tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(v));
                })])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Integer Steps (Percentage)"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Progress").SetContent(percentSlider), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("Selected: "), tss.UI.DeferSync$3(System.Int32, percentValue, function (v) {
                    return tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(System.String.format("{0}%", [H5.box(v, System.Int32)])));
                })])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Disabled"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Slider")).SetContent(new (tss.StepsSlider(System.String))(["Low", "Medium", "High"]).Disabled())])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TabbedModalSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null,
            _pivot: null,
            _modalCount: 0
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            init: function () {
                this._modalCount = 0;
            },
            ctor: function () {
                this.$initialize();
                this._pivot = tss.UI.Pivot();

                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TabbedModalSample, 5061, "A modal dialog with tabs"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("This sample demonstrates how to host Modals within a Pivot component, as well as how to use closeable tabs. Hosting a Modal within a Pivot allows it to embed its content while taking advantage of the Pivot's caching and lifecycle, and displaying a close button in the tab title automatically.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1("Open Closeable Modal").OnClick(H5.fn.bind(this, function (_, __) {
                    this.AddModalTab(true);
                })), tss.UI.Button$1("Open Non-Closeable Modal").OnClick(H5.fn.bind(this, function (_, __) {
                    this.AddModalTab(false);
                }))]), tss.ICX.H(tss.Pivot, tss.ICX.WS(tss.Pivot, this._pivot), 500)])).SetTitle("Usage")]));

                this.AddModalTab(true);
            }
        },
        methods: {
            AddModalTab: function (closeable) {
                this._modalCount = (this._modalCount + 1) | 0;
                var id = System.String.format("modal-{0}", [H5.box(this._modalCount, System.Int32)]);
                var titleText = System.String.format("Modal {0}", [H5.box(this._modalCount, System.Int32)]);

                var modal = tss.LayerExtensions.Content(tss.Modal, tss.UI.Modal(titleText), tss.ICX.Padding(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Regular(tss.txt, tss.UI.TextBlock(System.String.format("This is the content of {0}", [titleText]))), tss.ITFX.Small(tss.txt, tss.UI.TextBlock(System.String.format("It is embedded in the pivot and {0} be closed.", [(closeable ? "can" : "cannot")])))]), tss.usX.px$1(16)));

                tss.PivotX.Host(this._pivot, modal, id, tss.UI.PivotTitle$1(titleText, 686), closeable, function () {
                    console.log(System.String.format("Tab {0} was closed!", [id]));
                });

                this._pivot.Select(id);
            },
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TaskBoardSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var board = tss.UI.TaskBoard().OnColumnDrop(function (e) {
                    console.log(System.String.format("Column dropped: oldIndex={0}, newIndex={1}", H5.box(e.oldIndex, System.Int32), H5.box(e.newIndex, System.Int32)));
                }).OnColumnUpdate(function (e) {
                    console.log(System.String.format("Column updated: oldIndex={0}, newIndex={1}", H5.box(e.oldIndex, System.Int32), H5.box(e.newIndex, System.Int32)));
                }).Columns([tss.UI.TaskBoardColumn("To Do").OnCardAdd(function (e) {
                    console.log("Card added to To Do");
                }).OnCardRemove(function (e) {
                    console.log("Card removed from To Do");
                }).OnCardDrop(function (e) {
                    console.log("Card dropped in To Do");
                }).OnCardUpdate(function (e) {
                    console.log("Card updated in To Do");
                }).Cards([tss.UI.TaskBoardCard(tss.UI.TextBlock("Research user needs and identify key personas for the upcoming sprint.")).Header(tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.HStack().JustifyContent("space-between"), tss.usX.percent$1(100)), [tss.UI.Badge("Design").Primary(), tss.txtX.Secondary(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("T-101")))])).Footer(tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.HStack().JustifyContent("space-between"), tss.usX.percent$1(100)), [tss.ICX.Tooltip$1(tss.Icon, tss.UI.Class(tss.Icon, tss.ITFX.Small(tss.Icon, tss.UI.Icon$2(1221)), "tss-text-secondary"), "3 Comments"), tss.UI.Avatar(void 0, "JD").Size(Tesserae.AvatarSize.XSmall)])), tss.UI.TaskBoardCard(tss.UI.TextBlock("Draft UI mockups")), tss.UI.TaskBoardCard(tss.UI.TextBlock("Setup project repo")).Footer(tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.HStack().JustifyContent("flex-end"), tss.usX.percent$1(100)), [tss.UI.Avatar(void 0, "KL").Size(Tesserae.AvatarSize.XSmall)]))]), tss.UI.TaskBoardColumn("In Progress").OnCardAdd(function (e) {
                    console.log("Card added to In Progress");
                }).OnCardRemove(function (e) {
                    console.log("Card removed from In Progress");
                }).OnCardDrop(function (e) {
                    console.log("Card dropped in In Progress");
                }).OnCardUpdate(function (e) {
                    console.log("Card updated in In Progress");
                }).Cards([tss.UI.TaskBoardCard(tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Implement TaskBoard component"))).Header(tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.HStack().JustifyContent("space-between"), tss.usX.percent$1(100)), [tss.UI.Badge("Feature").Danger(), tss.txtX.Secondary(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("T-102")))])).Footer(tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.HStack().JustifyContent("space-between"), tss.usX.percent$1(100)), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Tooltip$1(tss.Icon, tss.UI.Class(tss.Icon, tss.ITFX.Small(tss.Icon, tss.UI.Icon$2(3273)), "tss-text-secondary"), "2 Attachments")]), tss.UI.Avatar(void 0, "MW").Size(Tesserae.AvatarSize.XSmall)])), tss.UI.TaskBoardCard(tss.UI.TextBlock("Add Playwright tests")).Header(tss.UI.Badge("Testing").Success())]), tss.UI.TaskBoardColumn("Done").OnCardAdd(function (e) {
                    console.log("Card added to Done");
                }).OnCardRemove(function (e) {
                    console.log("Card removed from Done");
                }).OnCardDrop(function (e) {
                    console.log("Card dropped in Done");
                }).OnCardUpdate(function (e) {
                    console.log("Card updated in Done");
                }).Cards([tss.UI.TaskBoardCard(tss.UI.TextBlock("Create project plan")).Footer(tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.HStack().JustifyContent("space-between"), tss.usX.percent$1(100)), [tss.txtX.Success(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Approved"))), tss.UI.Avatar(void 0, "AS").Size(Tesserae.AvatarSize.XSmall).Presence(Tesserae.AvatarPresence.Online)])), tss.UI.TaskBoardCard(tss.UI.TextBlock("Initial meeting"))])]);

                var toggle = tss.UI.IconToggle(System.Boolean, [tss.UI.IconToggleItem(System.Boolean, 4440, "Column Mode", false), tss.UI.IconToggleItem(System.Boolean, 4445, "Row Mode", true)]);
                toggle.AsObservable().tss$IOBS$System$Boolean$Observe(function (isRowMode) {
                    board.RowMode(isRowMode);
                });

                var readOnlyToggle = tss.UI.Toggle$1("Read Only").OnChange(function (s, e) {
                    board.ReadOnly(s.IsChecked);
                });

                this._content = tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TaskBoardSample, 1104, "A board for managing tasks"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("TaskBoard provides a Trello-like interface with draggable columns and cards. Use it for Kanban boards and task management."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [toggle, readOnlyToggle]), tss.ICX.Height(tss.TaskBoard, board, tss.usX.px$1(600))])).SetTitle("Overview")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TeachingSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var btn1 = tss.UI.Button$1("Feature A").Primary();
                var btn2 = tss.UI.Button$1("Feature B");
                var btn3 = tss.UI.Button$1("Feature C");

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TeachingSample, 4016, "An onboarding walkthrough that highlights UI elements one by one"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Teaching is a component for creating guided onboarding or instructional walkthroughs. It highlights specific UI elements sequentially, displaying a tooltip with contextual information at each step. Steps can require an explicit user action (clicking Next) or auto-advance after a fixed delay."), tss.UI.TextBlock("Use Teaching to help first-time users discover key features in a structured, low-friction way.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Teaching sparingly \u2014 only for genuinely complex or non-obvious workflows. Prefer NextButton steps for interactive demos so users stay in control. Avoid triggering a walkthrough every time the page loads; use RunIf with a condition (e.g. a first-run flag) so it only appears once. Keep tooltip copy short and action-oriented.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("3-Step Walkthrough Demo"), tss.ICX.PB(tss.txt, tss.UI.TextBlock("Click 'Start Walkthrough' to begin. Step 1 requires clicking Next, Step 2 auto-advances after 5 seconds, and Step 3 finishes with a confirmation toast."), 8), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [btn1, btn2, btn3]), tss.ICX.MT(tss.Button, tss.UI.Button$1("Start Walkthrough").SetIcon$1(3515), 16).OnClick$1(function () {
                    tss.UI.Teaching().AddStep(btn1, tss.UI.TextBlock("Step 1: This is Feature A. Click Next to continue."), "shift-toward-subtle", "top", tss.Teaching.StepType.NextButton).AddStep(btn2, tss.UI.TextBlock("Step 2: This is Feature B. It will auto-advance in 5 seconds."), "shift-toward-subtle", "top", tss.Teaching.StepType.After5seconds).AddStep(btn3, tss.UI.TextBlock("Step 3: This is Feature C. You're done!"), "shift-toward-subtle", "top", tss.Teaching.StepType.NextButton).OnComplete(function () {
                        tss.UI.Toast().Success("Walkthrough complete!");
                    }).RunNow();
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TextAreaSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TextAreaSample, 90, "A control to input multiple lines of text"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("TextAreas allow users to enter and edit multi-line text. They are commonly used for comments, descriptions, or any input that requires multiple lines of text.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use a TextArea when the expected input might be long or span multiple lines. Always pair it with a clear label. Consider using the AutoResize functionality to provide a better user experience without taking up too much initial screen real estate.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic TextAreas"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.TextArea$1()), tss.UI.Label$1("Placeholder").SetContent(tss.UI.TextArea$1().SetPlaceholder("Enter your description...")), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.TextArea$1("Disabled content").Disabled()), tss.UI.Label$1("Read-only").SetContent(tss.UI.TextArea$1("Read-only content").ReadOnly())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Auto Resize"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("AutoResize (allowShrink = true)").SetContent(tss.UI.TextArea$1("Type multiple lines here...").AutoResize(true, void 0, void 0)), tss.UI.Label$1("AutoResize (allowShrink = false)").SetContent(tss.UI.TextArea$1("Type multiple lines here...").AutoResize(false, void 0, void 0)), tss.UI.Label$1("AutoResize (minHeight = 100)").SetContent(tss.UI.TextArea$1("Type here...").AutoResize(true, 100, void 0)), tss.UI.Label$1("AutoResize (maxHeight = 150)").SetContent(tss.UI.TextArea$1("Type many lines to see scrolling...").AutoResize(true, void 0, 150))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.TextArea$1()), tss.UI.Label$1("Custom Error").SetContent(tss.ICVX.IsInvalid(tss.TextArea, tss.ICVX.Error(tss.TextArea, tss.UI.TextArea$1(), "Something went wrong")))])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TextBlockSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TextBlockSample, 4516, "A component to display text"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("TextBlock is the fundamental component for displaying text in Tesserae. It provides a consistent way to apply typography styles, sizes, and weights across your application."), tss.UI.TextBlock("It supports various built-in sizes, from tiny to mega, and different weights and colors.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use the predefined text sizes to maintain visual hierarchy. Use semi-bold or bold weights for headers and important information. Leverage the built-in color options (primary, success, danger, etc.) to convey meaning consistently. For long blocks of text, ensure the width is constrained for better readability. Use 'NoWrap' and text-overflow properties when dealing with limited space, such as in list items.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Text Sizes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Mega(tss.txt, tss.UI.TextBlock("Mega Text")), tss.ITFX.XXLarge(tss.txt, tss.UI.TextBlock("XXLarge Text")), tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock("XLarge Text")), tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Large Text")), tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock("MediumPlus Text")), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Medium Text (Default)")), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("SmallPlus Text")), tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Small Text")), tss.ITFX.XSmall(tss.txt, tss.UI.TextBlock("XSmall Text")), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("Tiny Text"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Weights and Colors"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Primary(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Bold Primary Text"))), tss.txtX.Success(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Semi-Bold Success Text"))), tss.txtX.Danger(tss.txt, tss.ITFX.Regular(tss.txt, tss.UI.TextBlock("Regular Danger Text")))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Wrapping and Overflow"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Default wrapping:")), tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."), tss.usX.px$1(300)), tss.ICX.MT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("No wrapping (ellipsis):")), 16), tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("This is a very long text that will be truncated with an ellipsis because it has NoWrap set and a constrained width.")), tss.usX.px$1(300))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Glow Effects"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Glow(tss.txt, tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Default Text"))), tss.txtX.Glow(tss.txt, tss.txtX.Danger(tss.txt, tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Danger Text")))), tss.txtX.Glow(tss.txt, tss.ISX.Foreground(tss.txt, tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Foreground Color")), tss.UI.Theme.Colors.Purple600)), tss.txtX.Glow(tss.txt, tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Custom Glow")), tss.UI.Theme.Colors.Lime300)])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TextBoxSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TextBoxSample, 4516, "A control to input text"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("TextBoxes allow users to enter and edit text. They are used in forms, search queries, and anywhere text input is required."), tss.UI.TextBlock("They support various modes like password input, read-only states, and built-in validation.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Always label your TextBoxes so users know what information is expected. Use placeholder text to provide a hint about the format or content. Mark required fields clearly. Use validation to provide immediate feedback on the correctness of the input. Use the appropriate input type (e.g., Password) for sensitive information. Provide a clear way to submit or clear the data.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic TextBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Placeholder").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your name...")), tss.UI.Label$1("Password").SetContent(tss.UI.TextBox$1().Password()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.TextBox$1("Disabled content").Disabled()), tss.UI.Label$1("Read-only").SetContent(tss.UI.TextBox$1("Read-only content").ReadOnly())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Must not be empty").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1(), function (tb) {
                    return System.String.isNullOrWhiteSpace(tb.Text) ? "This field is required" : null;
                })), tss.UI.Label$1("Positive Integer only").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1(), tss.Validation.NonZeroPositiveInteger)), tss.UI.Label$1("Custom Error").SetContent(tss.ICVX.IsInvalid(tss.TextBox, tss.ICVX.Error(tss.TextBox, tss.UI.TextBox$1(), "Something went wrong")))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBox$1().SetPlaceholder("Type and check toast...").OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Text changed to: {0}", [s.Text]));
                }), tss.UI.TextBox$1().SetPlaceholder("Search-like behavior...").OnInput(function (s, e) {
                    console.log(System.String.format("Current input: {0}", [s.Text]));
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded TextBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.TextBox, tss.UI.TextBox$1().SetPlaceholder("Small"), tss.BR.Small), tss.ISX.Rounded(tss.TextBox, tss.UI.TextBox$1().SetPlaceholder("Medium"), tss.BR.Medium), tss.ISX.Rounded(tss.TextBox, tss.UI.TextBox$1().SetPlaceholder("Full"), tss.BR.Full)])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TextBreadcrumbsSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TextBreadcrumbsSample, 120, "A breadcrumb navigation using text"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("TextBreadcrumbs are a navigational aid that indicates the current position within a hierarchy. They allow users to understand their context and easily navigate back to higher-level pages."), tss.UI.TextBlock("This component is typically placed at the top of a page, below the main navigation.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use breadcrumbs for applications with deep hierarchical structures. Place them consistently at the top of the content area. Use short, descriptive labels for each level. The last item in the breadcrumb should represent the current page and is typically not clickable. Breadcrumbs should complement, not replace, the primary navigation system.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Breadcrumbs"), tss.ICX.PB(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs().Items([tss.UI.TextBreadcrumb("Home").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Home clicked");
                }), tss.UI.TextBreadcrumb("Settings").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Settings clicked");
                }), tss.UI.TextBreadcrumb("Profile")]), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Overflow and Collapsing"), tss.UI.TextBlock("When the breadcrumbs exceed the available width, they automatically collapse into an overflow menu."), tss.ICX.PB(tss.TextBreadcrumbs, tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs(), tss.usX.px$1(300)).Items([tss.UI.TextBreadcrumb("Root").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Root");
                }), tss.UI.TextBreadcrumb("Folder 1").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Folder 1");
                }), tss.UI.TextBreadcrumb("Folder 2").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Folder 2");
                }), tss.UI.TextBreadcrumb("Subfolder A").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Subfolder A");
                }), tss.UI.TextBreadcrumb("Subfolder B").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Subfolder B");
                }), tss.UI.TextBreadcrumb("Current File")]), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Long Breadcrumb List"), tss.UI.TextBreadcrumbs().Items([tss.UI.TextBreadcrumb("Resources"), tss.UI.TextBreadcrumb("Images"), tss.UI.TextBreadcrumb("Icons"), tss.UI.TextBreadcrumb("UIcons"), tss.UI.TextBreadcrumb("Regular"), tss.UI.TextBreadcrumb("Arrows"), tss.UI.TextBreadcrumb("Chevron-Down.png")])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ThemeColorsSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                DumpTheme: function () {
                    var $t;
                    $t = H5.getEnumerator(System.Array.init([new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-default-background-color-root", "tss-default-background-hover-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-default-background-color-root", "tss-default-background-active-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-default-foreground-color-root", "tss-default-foreground-hover-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-default-foreground-color-root", "tss-default-foreground-active-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-primary-background-color-root", "tss-primary-border-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-primary-background-color-root", "tss-primary-background-hover-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-primary-background-color-root", "tss-primary-background-active-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-primary-foreground-color-root", "tss-primary-foreground-hover-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-primary-foreground-color-root", "tss-primary-foreground-active-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-danger-background-color-root", "tss-danger-border-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-danger-background-color-root", "tss-danger-background-hover-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-danger-background-color-root", "tss-danger-background-active-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-danger-foreground-color-root", "tss-danger-foreground-hover-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-danger-foreground-color-root", "tss-danger-foreground-active-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-scrollbar-track-color", "tss-scrollbar-track-hidden-color"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-scrollbar-track-color", "tss-scrollbar-thumb-color"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-scrollbar-track-color", "tss-scrollbar-thumb-hidden-color"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-default-border-color-root", "tss-default-background-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-dark-border-color-root", "tss-default-background-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-default-separator-color-root", "tss-default-background-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-progress-background-color-root", "tss-default-background-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-link-color-root", "tss-primary-background-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-tooltip-background-color-root", "tss-default-foreground-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-tooltip-foreground-color-root", "tss-default-background-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-tooltip-background-color-root", "tss-default-background-color-root"), new (System.ValueTuple$2(System.String,System.String)).$ctor1("tss-tooltip-foreground-color-root", "tss-default-foreground-color-root")], System.ValueTuple$2(System.String,System.String)));
                    try {
                        while ($t.moveNext()) {
                            var _d1 = $t.Current.$clone();
                            var fromC = { };
                            var toC = { };
                            H5.Deconstruct(_d1.$clone(), fromC, toC);
                            console.log(System.String.format("{0}: {1} to {2}: {3:n1}", (tss.UI.Theme.IsDark ? "DARK" : "LIGHT"), fromC.v, toC.v, H5.box(Tesserae.Tests.Samples.ThemeColorsSample.LightDiff(fromC.v, toC.v), System.Double, System.Double.format, System.Double.getHashCode)));
                        }
                    } finally {
                        if (H5.is($t, System.IDisposable)) {
                            $t.System$IDisposable$Dispose();
                        }
                    }
                },
                LightDiff: function (from, to) {
                    var fromVar = tss.Color.EvalVar("var(--" + (from || "") + ")");
                    var toVar = tss.Color.EvalVar("var(--" + (to || "") + ")");

                    if (!System.String.contains(fromVar,"(")) {
                        fromVar = "rgb(" + (fromVar || "") + ")";
                    }

                    if (!System.String.contains(toVar,"(")) {
                        toVar = "rgb(" + (toVar || "") + ")";
                    }
                    var fromColor = tss.HSLColor.op_Implicit$1(tss.Color.FromString(fromVar));
                    var toColor = tss.HSLColor.op_Implicit$1(tss.Color.FromString(toVar));
                    return toColor.Luminosity - fromColor.Luminosity;
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var currentTheme = tss.UI.Theme.IsLight;
                var primaryLight = new (tss.SettableObservableT(tss.Color))();
                var backgroundLight = new (tss.SettableObservableT(tss.Color))();
                var primaryDark = new (tss.SettableObservableT(tss.Color))();
                var backgroundDark = new (tss.SettableObservableT(tss.Color))();

                var combined = new (tss.CombinedObservableT4(tss.Color,tss.Color,tss.Color,tss.Color))(primaryLight, primaryDark, backgroundLight, backgroundDark);

                var cpPrimaryLight = tss.UI.ColorPicker().OnInput(function (cp, ev) {
                    primaryLight.Value$1 = cp.Color;
                });
                var cpPrimaryDark = tss.UI.ColorPicker().OnInput(function (cp, ev) {
                    primaryDark.Value$1 = cp.Color;
                });
                var cpBackgroundLight = tss.UI.ColorPicker().OnInput(function (cp, ev) {
                    backgroundLight.Value$1 = cp.Color;
                });
                var cpBackgroundDark = tss.UI.ColorPicker().OnInput(function (cp, ev) {
                    backgroundDark.Value$1 = cp.Color;
                });

                tss.UI.Theme.Light();

                window.setTimeout(function (_) {

                    primaryLight.Value$1 = tss.Color.FromString(tss.Color.EvalVar(tss.UI.Theme.Primary.Background));
                    backgroundLight.Value$1 = tss.Color.FromString(tss.Color.EvalVar(tss.UI.Theme.Default.Background));

                    tss.UI.Theme.Dark();

                    window.setTimeout(function (__) {
                        primaryDark.Value$1 = tss.Color.FromString(tss.Color.EvalVar(tss.UI.Theme.Primary.Background));
                        backgroundDark.Value$1 = tss.Color.FromString(tss.Color.EvalVar(tss.UI.Theme.Default.Background));
                        tss.UI.Theme.IsLight = currentTheme;


                        cpPrimaryLight.Color = primaryLight.Value$1;
                        cpPrimaryDark.Color = primaryDark.Value$1;
                        cpBackgroundLight.Color = backgroundLight.Value$1;
                        cpBackgroundDark.Color = backgroundDark.Value$1;

                        combined.ObserveFutureChanges(function (v) {
                            tss.UI.Theme.SetPrimary(v.Item1, v.Item2);
                            tss.UI.Theme.SetBackground(v.Item3, v.Item4);
                        });

                    }, 1);
                }, 1);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ThemeColorsSample, 3262, "A utility to display theme colors"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ThemeColors allows for real-time inspection and customization of the application's theme. It provides a detailed view of the primary, secondary, and semantic colors used throughout the UI, and allows you to experiment with different primary and background color combinations for both light and dark modes.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use this sample to verify the accessibility and contrast of your theme choices. Ensure that primary and background colors provide sufficient contrast for readability in both light and dark themes. Changes made here are applied immediately to the entire application, allowing for rapid prototyping of different brand identities.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem), tss.UI.DetailsList(Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem, [tss.UI.DetailsListColumn("ThemeName", tss.usX.px$1(120), false, false, void 0, void 0), tss.UI.DetailsListColumn("Background", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("Foreground", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("Border", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("BackgroundActive", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("BackgroundHover", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("ForegroundActive", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("ForegroundHover", tss.usX.px$1(160), false, false, void 0, void 0)]).Compact(), tss.usX.px$1(500)).WithListItems(System.Array.init([new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Default"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Primary"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Secondary"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Success"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Danger")], Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem)).SortedBy("Name"), tss.UI.Label$1("Primary Light").Inline().SetContent(cpPrimaryLight), tss.UI.Label$1("Primary Dark").Inline().SetContent(cpPrimaryDark), tss.UI.Label$1("Background Light").Inline().SetContent(cpBackgroundLight), tss.UI.Label$1("Background Dark").Inline().SetContent(cpBackgroundDark)])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TimeHistogramPickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                PickerDemo: function (values, maxBuckets, renderTime, showBucketTooltipOnHover) {
                    if (renderTime === void 0) { renderTime = null; }
                    if (showBucketTooltipOnHover === void 0) { showBucketTooltipOnHover = true; }
                    var selection = new (tss.SettableObservableT(System.String))();
                    var picker = tss.UI.TimeHistogramPicker(values, maxBuckets).WithCustomTimeRender(renderTime).ShowBucketTooltipOnHover(showBucketTooltipOnHover).OnRangeChanged(function (from, to, count) {
                        selection.Value$1 = Tesserae.Tests.Samples.TimeHistogramPickerSample.FormatSelection(from, to, count, renderTime);
                    });

                    selection.Value$1 = Tesserae.Tests.Samples.TimeHistogramPickerSample.FormatSelection(picker.SelectedFrom, picker.SelectedTo, picker.SelectedCount, renderTime);

                    return tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [picker, tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Selected: ")), tss.UI.DeferSync$3(System.String, selection, function (value) {
                        return tss.UI.TextBlock(value);
                    })])]), 24);
                },
                BucketPickerDemo: function (buckets) {
                    var selection = new (tss.SettableObservableT(System.String))();
                    var picker = tss.UI.TimeHistogramPicker$1(buckets).OnRangeChanged(function (from, to, count) {
                        selection.Value$1 = Tesserae.Tests.Samples.TimeHistogramPickerSample.FormatSelection(from, to, count);
                    });

                    selection.Value$1 = Tesserae.Tests.Samples.TimeHistogramPickerSample.FormatSelection(picker.SelectedFrom, picker.SelectedTo, picker.SelectedCount);

                    return tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [picker, tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Selected: ")), tss.UI.DeferSync$3(System.String, selection, function (value) {
                        return tss.UI.TextBlock(value);
                    })])]), 24);
                },
                FormatSelection: function (from, to, count, renderTime) {
                    if (renderTime === void 0) { renderTime = null; }
                    if (count === 0) {
                        return "No values";
                    }

                    renderTime = renderTime || Tesserae.Tests.Samples.TimeHistogramPickerSample.DefaultRenderTime;
                    return System.String.format("{0} - {1} ({2:n0} values)", renderTime(from), renderTime(to), H5.box(count, System.Int32));
                },
                DefaultRenderTime: function (time) {
                    return System.DateTime.format(time, "g");
                },
                GetSmallValues: function () {
                    var now = System.DateTime.getNow();
                    return System.Array.init([System.DateTime.addMinutes(now, 30), System.DateTime.addMinutes(now, -15), System.DateTime.addHours(now, 2)], System.DateTime);
                },
                GetDenseValues: function () {
                    var start = System.DateTime.addHours(System.DateTime.getToday(), 8);
                    return System.Linq.Enumerable.range(0, 720).selectMany(function (i) {
                        return System.Linq.Enumerable.range(0, ((1 + (i % 9)) | 0)).select(function (j) {
                            return System.DateTime.addSeconds(System.DateTime.addMinutes(start, i), H5.Int.mul(j, 7));
                        });
                    }).ToArray(System.DateTime);
                },
                GetFineGrainedValues: function () {
                    var start = System.DateTime.addHours(System.DateTime.getToday(), 14);
                    return System.Linq.Enumerable.range(0, 360).selectMany(function (second) {
                        var count = second % 45 < 9 ? 8 : second % 30 < 4 ? 5 : second % 11 === 0 ? 3 : 1;
                        return System.Linq.Enumerable.range(0, count).select(function (index) {
                            return System.DateTime.addMilliseconds(System.DateTime.addSeconds(start, second), H5.Int.mul(index, 90));
                        });
                    }).ToArray(System.DateTime);
                },
                GetMultiYearValues: function () {
                    var start = System.DateTime.addYears(System.DateTime.getToday(), -6);
                    return System.Linq.Enumerable.range(0, 180).select(function (i) {
                        return System.DateTime.addHours(System.DateTime.addDays(start, (H5.Int.mul(i, 17)) % 2190), (H5.Int.mul(i, 11)) % 24);
                    }).ToArray(System.DateTime);
                },
                GetGappedClusterValues: function () {
                    var start = System.DateTime.addYears(System.DateTime.getToday(), -4);

                    var tinyBurst = System.Linq.Enumerable.range(0, 18).select(function (i) {
                        return System.DateTime.addSeconds(start, H5.Int.mul(i, 11));
                    });

                    var releaseCluster = System.Linq.Enumerable.range(0, 240).select(function (i) {
                        return System.DateTime.addSeconds(System.DateTime.addMinutes(System.DateTime.addMonths(start, 9), H5.Int.mul(i, 5)), i % 13);
                    });

                    var sparseReview = System.Linq.Enumerable.range(0, 9).select(function (i) {
                        return System.DateTime.addHours(System.DateTime.addDays(System.DateTime.addMonths(start, 18), H5.Int.mul(i, 3)), i % 5);
                    });

                    var denseMigration = System.Linq.Enumerable.range(0, 1200).select(function (i) {
                        return System.DateTime.addSeconds(System.DateTime.addMinutes(System.DateTime.addYears(start, 3), i % 180), ((H5.Int.div(i, 180)) | 0));
                    });

                    var longTail = System.Linq.Enumerable.range(0, 45).select(function (i) {
                        return System.DateTime.addHours(System.DateTime.addDays(System.DateTime.addYears(start, 4), H5.Int.mul(i, 2)), i % 7);
                    });

                    return denseMigration.concat(tinyBurst).concat(longTail).concat(releaseCluster).concat(sparseReview).ToArray(System.DateTime);
                },
                GetDailyBackendBuckets: function () {
                    var start = System.DateTime.addDays(System.DateTime.getToday(), -18);
                    var counts = System.Array.init([12, 0, 3, 45, 0, 0, 18, 95, 30, 0, 4, 8, 0, 150, 70, 0, 22, 6, -5], System.Int32);

                    return System.Linq.Enumerable.from(counts, System.Int32).select(function (count, i) {
                            return new tss.TimeHistogramBucket.$ctor1(System.DateTime.addDays(start, i), System.DateTime.addDays(start, ((i + 1) | 0)), count);
                        }).reverse().ToArray(tss.TimeHistogramBucket);
                },
                GetLongRangeBackendBuckets: function () {
                    var start = System.DateTime.addYears(System.DateTime.getToday(), -8);
                    var counts = System.Array.init([20, 0, 45, 140, 12, 0, 0, 260, 410, 90, 0, 35, 75, 0, 510, 180], System.Int32);

                    return System.Linq.Enumerable.from(counts, System.Int32).select(function (count, i) {
                            var bucketStart = System.DateTime.addMonths(start, H5.Int.mul(i, 6));
                            return new tss.TimeHistogramBucket.$ctor1(bucketStart, System.DateTime.addMonths(bucketStart, 6), count);
                        }).ToArray(tss.TimeHistogramBucket);
                },
                GetLargeValues: function () {
                    var start = System.DateTime.addYears(System.DateTime.getToday(), -1);
                    var minutesInYear = 525600;
                    return System.Linq.Enumerable.range(0, 100000).select(function (i) {
                        return System.DateTime.addSeconds(System.DateTime.addMinutes(start, (H5.Int.mul(i, 37)) % minutesInYear), i % 60);
                    }).ToArray(System.DateTime);
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TimeHistogramPickerSample, 929, "A histogram control for selecting a time range"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("The TimeHistogramPicker turns a DateTime array into adaptive buckets and lets users narrow the selected range from either side."), tss.UI.TextBlock("It sorts a private copy of the input values, so callers can pass unsorted data without changing their source array.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use the picker when users need to filter a time-based dataset while keeping the shape and density of the data visible. The component adapts from second-level ranges through multi-year spans and is designed for large arrays.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Three Values"), Tesserae.Tests.Samples.TimeHistogramPickerSample.PickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetSmallValues(), 12), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dense Minute and Hour Data"), Tesserae.Tests.Samples.TimeHistogramPickerSample.PickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetDenseValues(), 64), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Fine-grained Seconds"), Tesserae.Tests.Samples.TimeHistogramPickerSample.PickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetFineGrainedValues(), 360, function (date) {
                    return System.DateTime.format(date, "HH:mm:ss");
                }, true), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Time Rendering"), Tesserae.Tests.Samples.TimeHistogramPickerSample.PickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetDenseValues(), 48, function (date) {
                    return System.DateTime.format(date, "MMM d, HH:mm");
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Sparse Multi-year Data"), Tesserae.Tests.Samples.TimeHistogramPickerSample.PickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetMultiYearValues(), 48), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Gapped Clusters with Uneven Groups"), Tesserae.Tests.Samples.TimeHistogramPickerSample.PickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetGappedClusterValues(), 80), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Large Dataset (100,000 Values)"), Tesserae.Tests.Samples.TimeHistogramPickerSample.PickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetLargeValues(), 80)])).SetTitle("Usage")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Daily Buckets from Backend"), Tesserae.Tests.Samples.TimeHistogramPickerSample.BucketPickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetDailyBackendBuckets()), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Long-range Aggregated Buckets from Backend"), Tesserae.Tests.Samples.TimeHistogramPickerSample.BucketPickerDemo(Tesserae.Tests.Samples.TimeHistogramPickerSample.GetLongRangeBackendBuckets())])).SetTitle("Backend Precomputed Buckets")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TimelineSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.TimelineSample, 1110, "A component to display a timeline"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Timeline displays a series of events in chronological order, using a vertical line to connect them."), tss.UI.TextBlock("It is ideal for activity feeds, version histories, or any process where the sequence of steps is important.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Timelines to show the progression of time or a sequence of related events. Clearly distinguish between past, current, and future events if applicable. Use the 'SameSide' property if you want a more linear, left-aligned layout, or the default staggered layout for a more balanced visual look. Ensure that each event has a clear timestamp and a concise description.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Default Staggered Timeline"), tss.ICX.MB(tss.Timeline, tss.ICX.Height(tss.Timeline, this.BuildTimeline(tss.UI.Timeline()), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Same Side Alignment"), tss.ICX.MB(tss.Timeline, tss.ICX.Height(tss.Timeline, this.BuildTimeline(tss.UI.Timeline().SameSide()), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Constrained Width"), tss.ICX.Height(tss.Timeline, this.BuildTimeline(tss.UI.Timeline().TimelineWidth(tss.usX.px$1(400))), tss.usX.px$1(400))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            BuildTimeline: function (timeline) {
                timeline.Add$1(tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()).JustifyContent("space-between"), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Build #4182 succeeded")), tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("2m ago")), tss.UI.Theme.Secondary.Foreground)]), tss.ICX.PT(tss.txt, tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("All 47 tests passing on master.")), tss.UI.Theme.Secondary.Foreground), 8)]), tss.UI.Theme.Success.Background);

                timeline.Add$1(tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()).JustifyContent("space-between"), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Anna pushed to master")), tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("14m ago")), tss.UI.Theme.Secondary.Foreground)]), tss.ICX.PT(tss.txt, tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Merge #312: Add ProgressRing component (3 files changed).")), tss.UI.Theme.Secondary.Foreground), 8)]), tss.UI.Theme.Primary.Background);

                timeline.Add$1(tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()).JustifyContent("space-between"), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Build #4181 failed")), tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("1h ago")), tss.UI.Theme.Secondary.Foreground)]), tss.ICX.PT(tss.txt, tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("2 of 47 tests failed in Tesserae.Tests/Buttons.")), tss.UI.Theme.Secondary.Foreground), 8)]), tss.UI.Theme.Danger.Background);

                timeline.Add$1(tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()).JustifyContent("space-between"), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Index rebuilt")), tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("yesterday")), tss.UI.Theme.Secondary.Foreground)]), tss.ICX.PT(tss.txt, tss.ISX.Foreground(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("4,182 documents reindexed from curiosity-prod.")), tss.UI.Theme.Secondary.Foreground), 8)]), tss.UI.Theme.Default.DarkBorder);

                return timeline;
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TimePickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TimePickerSample, 1110, "A control to select a time of day"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("TimePicker lets users select a time of day using the browser's native time-input widget. It exposes the selected value as a DateTimeOffset, making it straightforward to combine with a date or to compute durations."), tss.UI.TextBlock("It is ideal for scheduling meetings, setting reminders, configuring cron-like triggers, or any feature where the user must specify a wall-clock time.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use TimePicker whenever the domain requires a specific time rather than a duration. When presenting times across time zones, always display the relevant zone label next to the control. For coarse scheduling (morning / afternoon), prefer a ChoiceGroup or Dropdown to keep the UI lightweight. Consider pairing TimePicker with a DatePicker when a full date-time is needed rather than using a separate DateTimePicker, as two focused controls can be clearer in form contexts.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic TimePicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("No default (empty)").SetContent(new tss.TimePicker().OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0:t}", [s.Time.$clone()]));
                })), tss.UI.Label$1("Pre-filled with current time").SetContent(new tss.TimePicker(System.DateTimeOffset.Now.$clone()).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0:t}", [s.Time.$clone()]));
                })), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(new tss.TimePicker(System.DateTimeOffset.Now.$clone()).Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), new tss.TimePicker(System.DateTimeOffset.Now.$clone()).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Time changed to {0:T}", [s.Time.$clone()]));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TippySample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var stack = tss.UI.SectionStack().Secondary();
                var countSlider = tss.UI.Slider(5, 0, 10, 1);

                var size = new (tss.SettableObservableT(System.Int32))();
                var deferedWithChangingSize = tss.ICX.WS(tss.IDefer, tss.UI.DeferSync$3(System.Int32, size, function (sz) {
                    return tss.ICX.H(tss.Button, tss.UI.Button$1(System.String.format("Height = {0:n0}px", [H5.box(sz, System.Int32)])), sz);
                }));
                var _discard1 = { };

                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TippySample, 1221, "A utility to display tippy tooltips"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Tippy is the underlying engine for tooltips and popovers in Tesserae. It provides a flexible way to attach rich, interactive content to any component, with support for various animations, placements, and trigger events.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use tooltips to provide additional context or information without cluttering the main UI. Keep text tooltips brief and focused. For interactive tooltips, ensure the content is easy to use and provides clear affordances. Utilize animations sparingly to enhance the user experience without being distracting. Always consider the placement of the tooltip to ensure it doesn't obscure relevant content.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.Tooltip$1(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Hover me"), 200), "This is a simple text tooltip"), tss.ICX.Tooltip$1(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Animated Tooltip"), 200), "This is a simple text tooltip with animations", "shift-away-subtle"), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Interactive Tooltip"), 200), tss.UI.Button$1("Click me").OnClick$1(function () {
                    tss.UI.Toast().Success("You clicked!");
                }), true, "none", "top", 250, 0, true, false, 350, true, false, void 0, void 0), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Defers on Tooltips"), 200), deferedWithChangingSize), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Nested Tooltips"), 200), tss.UI.Button$1("Click me").OnClick(function (b1, _) {
                    tss.tippy.ShowFor$1(b1, tss.UI.Button$1("Click me").OnClick$1(function () {
                        tss.UI.Toast().Success("You clicked!");
                    }), _discard1);
                }), true, "none", "top", 250, 0, true, false, 350, true, false, void 0, void 0)])])).SetTitle("Usage")]));
                tss.ICX.WhenMounted(tss.IC, this.content, H5.fn.bind(this, function () {
                    var rng = new System.Random.ctor();
                    var repeat = window.setInterval(function (_) {
                        size.Value$1 = (H5.Int.mul(rng.Next$2(0, 10), 50) + 50) | 0;
                    }, 1000);
                    tss.ICX.WhenRemoved(tss.IC, this.content, function () {
                        window.clearInterval(repeat);
                    });
                }));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ToastSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ToastSample, 1737, "A utility to display toast notifications"), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Toasts are short-lived, non-intrusive notifications that provide feedback about an operation. They appear temporarily on the screen and then disappear automatically, making them ideal for success messages, warnings, or simple information updates.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Toasts for brief, informative messages that don't require user action. Keep the text short and recognizable. Ensure the Toast duration is long enough to be read but short enough not to become an annoyance. Avoid overloading the user with too many simultaneous Toasts. For critical errors that require immediate attention or user interaction, use a Dialog or Modal instead.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top-right (default)"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top left"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().TopLeft().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().TopLeft().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().TopLeft().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().TopLeft().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom right"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().BottomRight().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().BottomRight().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().BottomRight().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().BottomRight().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom left"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().BottomLeft().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().BottomLeft().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().BottomLeft().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().BottomLeft().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top center with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().TopCenter().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().TopCenter().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().TopCenter().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().TopCenter().Error$1("This is a title", "Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top full with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().TopFull().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().TopFull().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().TopFull().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().TopFull().Error$1("This is a title", "Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom center with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().BottomCenter().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().BottomCenter().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().BottomCenter().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().BottomCenter().Error$1("This is a title", "Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom full with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
                    tss.UI.Toast().BottomFull().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick$1(function () {
                    tss.UI.Toast().BottomFull().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick$1(function () {
                    tss.UI.Toast().BottomFull().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick$1(function () {
                    tss.UI.Toast().BottomFull().Error$1("This is a title", "Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toast as banner"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info on top").OnClick$1(function () {
                    tss.UI.Toast().TopFull().Banner().Information$1("This is a banner", "Info!");
                }), tss.UI.Button$1().SetText("Success on top").OnClick$1(function () {
                    tss.UI.Toast().TopFull().Banner().Success$1("This is a banner", "Success!");
                }), tss.UI.Button$1().SetText("Warning on bottom").OnClick$1(function () {
                    tss.UI.Toast().BottomFull().Banner().Warning$1("This is a banner", "Warning!");
                }), tss.UI.Button$1().SetText("Error on bottom").OnClick$1(function () {
                    tss.UI.Toast().BottomFull().Banner().Error$1("This is a banner", "Error!");
                })])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ToggleSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ToggleSample, 4599, "A control to toggle between two states"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A Toggle represents a physical switch that allows users to choose between two mutually exclusive options, typically 'on' and 'off'."), tss.UI.TextBlock("Unlike a Checkbox, a Toggle is intended for immediate actions where the change takes effect as soon as the switch is flipped.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use Toggles for binary settings that have an immediate effect (e.g., turning Wi-Fi on or off). Labels should be short and describe the setting clearly. Avoid using Toggles when a user needs to click a 'Submit' or 'Apply' button to save changes; use a Checkbox instead. Ensure that the 'on' and 'off' states are visually distinct and easy to understand at a glance.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Toggles"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Default (Unchecked)").SetContent(tss.UI.Toggle()), tss.UI.Label$1("Checked").SetContent(tss.UI.Toggle().Checked()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Checked")).SetContent(tss.UI.Toggle().Checked().Disabled()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Unchecked")).SetContent(tss.UI.Toggle().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Labels and Inline"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Toggle().SetText("With Label"), tss.UI.Toggle$3(tss.UI.TextBlock("Visible"), tss.UI.TextBlock("Hidden")), tss.UI.Label$1("Inline Toggle").Inline().SetContent(tss.UI.Toggle())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.Toggle().SetText("Feature X").OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Feature X is now {0}", [(s.IsChecked ? "Enabled" : "Disabled")]));
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Formatting"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Tiny(tss.Toggle, tss.UI.Toggle$1("Tiny")), tss.ITFX.Small(tss.Toggle, tss.UI.Toggle$1("Small (default)")), tss.ITFX.SmallPlus(tss.Toggle, tss.UI.Toggle$1("Small Plus")), tss.ITFX.Medium(tss.Toggle, tss.UI.Toggle$1("Medium")), tss.ITFX.Large(tss.Toggle, tss.UI.Toggle$1("Large")), tss.ITFX.XLarge(tss.Toggle, tss.UI.Toggle$1("XLarge")), tss.ITFX.XXLarge(tss.Toggle, tss.UI.Toggle$1("XXLarge")), tss.ITFX.Mega(tss.Toggle, tss.UI.Toggle$1("Mega")), tss.ITFX.Bold(tss.Toggle, tss.UI.Toggle$1("Bold text"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Toggles"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Small").SetContent(tss.ISX.Rounded(tss.Toggle, tss.UI.Toggle(), tss.BR.Small)), tss.UI.Label$1("Medium").SetContent(tss.ISX.Rounded(tss.Toggle, tss.UI.Toggle(), tss.BR.Medium)), tss.UI.Label$1("Full").SetContent(tss.ISX.Rounded(tss.Toggle, tss.UI.Toggle(), tss.BR.Full))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Icon Toggle"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.IconToggle(System.String, [tss.UI.IconToggleItem(System.String, 1221, "Chat", "Chat"), tss.UI.IconToggleItem(System.String, 3936, "Search", "Search")]), tss.UI.IconToggle(System.String, [tss.UI.IconToggleItem(System.String, 150, "Apple", "Apple"), tss.UI.IconToggleItem(System.String, 347, "Banana", "Banana"), tss.UI.IconToggleItem(System.String, 3228, "Orange Juice", "Orange Juice"), tss.UI.IconToggleItem(System.String, 655, "Bread", "Bread")])])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ToolCallSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ToolCallSample, 4615, "Inline tool-call indicators and a multi-tool summary popup"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("ToolCall renders a single tool invocation inline. It behaves like an accordion: a compact header with an icon and label, expanding to reveal arbitrary content the first time it is clicked (the content component is created lazily)."), tss.UI.TextBlock("ToolsUsed groups many ToolCalls behind a compact summary. Clicking it opens a popup with the list of tools on the left; selecting one slides to the detail view on the right, with a back button to return to the list.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Inline ToolCall"), tss.UI.TextBlock("A single expandable tool call. The content factory only runs when the user first expands it."), tss.UI.ToolCall(4510, "Bash ls -la && git status", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("total 16\ndrwxr-xr-x  3 user user 4096 Jan 1 12:00 .\n\nOn branch main\nnothing to commit, working tree clean"));
                }), tss.UI.ToolCall(1749, "Read /home/user/project/README.md", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("# My Project\n\nA sample project demonstrating the ToolCall component.\n\n## Usage\n\n..."));
                }).Expanded(), tss.UI.ToolCall(3936, "Grep \"useEffect\" src/", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("src/App.tsx:5: import { useEffect } from 'react';\nsrc/hooks/useData.ts:1: import { useEffect, useState } from 'react';"));
                }), tss.UI.ToolCall(2774, "Update todos").NotExpandable(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("ToolsUsed summary popup"), tss.UI.TextBlock("When an AI uses many tools, surface a compact summary that opens a list/detail popup, similar to a master-detail navigation on mobile."), tss.UI.ToolsUsed([tss.UI.ToolCall(4510, "Bash ls -la && git status && git branch --show-current", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("total 348\ndrwxr-xr-x ...\nOn branch claude/add-tool-components\nnothing to commit, working tree clean"));
                }), tss.UI.ToolCall(4510, "Bash cat Needle.slnx && echo \"---\" && ls src/ && ...", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("<Solution>...\n---\nNeedle/\nNeedle.Tests/"));
                }), tss.UI.ToolCall(4510, "Bash ls src/Needle/ && echo \"---\" && ls tests/N...", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("Inference/\nModel/\nTokenizer/\n---\nIntegration/\nUnit/"));
                }), tss.UI.ToolCall(1749, "Read /home/user/needle/README.md", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("# Needle\n\nA tiny ML library written in C#."));
                }), tss.UI.ToolCall(4510, "Bash cat src/Needle/Needle.csproj && echo \"---\"", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("<Project Sdk=\"Microsoft.NET.Sdk\">...</Project>"));
                }), tss.UI.ToolCall(4510, "Bash find src/Needle -type f | head -50", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("src/Needle/Needle.csproj\nsrc/Needle/Inference/Runner.cs\n..."));
                }), tss.UI.ToolCall(4510, "Bash find tests -type f && echo \"---\" && find n...", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("tests/Needle/Integration/RunnerTests.cs\n---"));
                }), tss.UI.ToolCall(1749, "Read /home/user/needle/src/Needle/Inference/Run...", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("namespace Needle.Inference;\n\npublic class Runner { ... }"));
                }), tss.UI.ToolCall(1749, "Read /home/user/needle/src/Needle/Weights/Weigh...", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("namespace Needle.Weights;\n\npublic class Weights { ... }"));
                }), tss.UI.ToolCall(1749, "Read /home/user/needle/src/Needle/Model/NeedleM...", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("namespace Needle.Model;\n\npublic class NeedleModel { ... }"));
                }), tss.UI.ToolCall(4615, "ToolSearch max_results, query", function () {
                    return tss.UI.TextBlock("Found 3 candidate tools matching query 'tokenizer'.");
                }), tss.UI.ToolCall(2774, "Update todos").NotExpandable(), tss.UI.ToolCall(1749, "Read /home/user/needle/needle/model/run.py", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("import torch\n\ndef run(model, x): ..."));
                }), tss.UI.ToolCall(1749, "Read /home/user/needle/src/Needle/Tokenizer/Nee...", function () {
                    return tss.txtX.BreakSpaces(tss.txt, tss.UI.TextBlock("namespace Needle.Tokenizer;\n\npublic class NeedleTokenizer { ... }"));
                })]).SetSummary("Ran 14 commands, read 9 files, used a tool").SetTitle("Tools used")])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TreeSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TreeSample, 4075, "A component that displays a hierarchical list"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("A tree displays hierarchical data. Nodes can be expanded or collapsed to reveal nested data."), tss.UI.TextBlock("Supports synchronous and asynchronous loading of child nodes.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Synchronous Tree"), new tss.Tree().Items([new tss.Tree.Item("samples/ConsoleApp...", 2004).Expanded().Items([new tss.Tree.Item("ConsoleApp1.csproj", 1864).Selected(), new tss.Tree.Item("Program.cs", 1864)]), new tss.Tree.Item("src", 2004).Expanded().Items([new tss.Tree.Item("MarkdownRende...", 2004).Expanded().Items([new tss.Tree.Item("MarkdownConve...", 1864), new tss.Tree.Item("Slides", 2004).Expanded().Items([new tss.Tree.Item("Blocks", 2004).Expanded().Items([new tss.Tree.Item("HeadingRe...", 1864), new tss.Tree.Item("HeadingRe...", 1864)]), new tss.Tree.Item("SlideDocume...", 1864)])]), new tss.Tree.Item("MarkdownRende...", 2004).Expanded().Items([new tss.Tree.Item("MarkdownRende...", 1864), new tss.Tree.Item("Program.cs", 1864)]), new tss.Tree.Item("MarkdownRenderer...", 1864)])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Asynchronous Tree"), new tss.Tree().Items([new tss.Tree.Item("Lazy Loaded Folder", 2004).ItemsAsync(function () {
                    var $tcs = new H5.TCS();
                    (async () => {
                        {
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(1000)));
                            return System.Array.init([new tss.Tree.Item("Async Child 1", 1864), new tss.Tree.Item("Async Child 2", 1864)], tss.Tree.Item);
                        }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Selectable Tree"), new tss.Tree().SelectionEnabled().Items([new tss.Tree.Item("Root 1", 2004).Expanded().Items([new tss.Tree.Item("Child A", 1864), new tss.Tree.Item("Child B", 1864)]), new tss.Tree.Item("Root 2", 2004).Expanded().Items([new tss.Tree.Item("Child C", 1864).Selected(), new tss.Tree.Item("Child D", 1864)])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Tree with Commands and Context Menu"), new tss.Tree().Items([new tss.Tree.Item("src", 2004, [new Tesserae.TreeCommand.$ctor7(3536).Tooltip$1("Add file").OnClick(function () {
                    window.alert("Add file clicked");
                }), new Tesserae.TreeCommand.$ctor7(3728).Tooltip$1("Refresh").OnClick(function () {
                    window.alert("Refresh clicked");
                })]).Expanded().Items([new tss.Tree.Item("Program.cs", 1864, [new Tesserae.TreeCommand.$ctor7(3335).Tooltip$1("Rename").OnClick(function () {
                    window.alert("Rename Program.cs");
                }), new Tesserae.TreeCommand.$ctor7(4675).Tooltip$1("Delete").OnClick(function () {
                    window.alert("Delete Program.cs");
                })]), new tss.Tree.Item("README.md", 1864, [new Tesserae.TreeCommand.$ctor7(2944).Tooltip$1("Context menu").HookToParentContextMenu().OnClick(function () {
                    window.alert("README.md context action (right-click or button)");
                })]), new tss.Tree.Item("notes.txt", 1864).OnContextMenu(function (s, e) {
                    e.preventDefault();
                    window.alert("Right-clicked notes.txt");
                }), new tss.Tree.Item("config.json", 1864, [new Tesserae.TreeCommand.$ctor7(2943).Tooltip$1("More actions").OnClickMenu(function () {
                    return System.Array.init([new Tesserae.TreeCommand.$ctor7(3335).SetText("Rename").OnClick(function () {
                        window.alert("Rename config.json");
                    }), new Tesserae.TreeCommand.$ctor7(1303).SetText("Duplicate").OnClick(function () {
                        window.alert("Duplicate config.json");
                    }), new Tesserae.TreeCommand.$ctor7(4675).SetText("Delete").Danger().OnClick(function () {
                        window.alert("Delete config.json");
                    })], Tesserae.TreeCommand);
                })])])])])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.TutorialModalSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                SampleTutorialModal: function () {
                    var tutorialModal = { };
                    return tss.UI.Var(tss.TutorialModal, tss.UI.TutorialModal(), tutorialModal).SetTitle("This is a Tutorial Modal").SetHelpText("Lorem ipsum dolor sit amet, consectetur adipiscing elit,<b> sed do </b> eiusmod tempor incididunt ut labore et dolore magna aliqua. ", true).SetImageSrc("./assets/img/box-img.svg", tss.usX.px$1(16)).SetContent(tss.ICTX.Children$6(tss.S, tss.ScrollBar.ScrollY(tss.S, tss.ICX.S(tss.S, tss.UI.VStack())), [tss.UI.Label$1("Input 1").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 2").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 3").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 4").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 5").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 6").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 7").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 8").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here...")), tss.UI.Label$1("Input 9").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your input here..."))])).SetFooterCommands([tss.UI.Button$1("Discard").OnClick(function (_, __) {
                        tutorialModal.v.Hide();
                    }), tss.UI.Button$1("Save").Primary().OnClick(function (_, __) {
                        tutorialModal.v.Hide();
                    })]);
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var container = tss.UI.Raw$1();

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.TutorialModalSample, 2167, "A modal dialog for tutorials"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("TutorialModal is a specialized modal designed for guided processes, such as onboarding or feature walkthroughs. It combines a large content area with a dedicated help panel and an optional illustrative image, providing a structured environment for users to learn while they interact.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use TutorialModals for complex tasks that benefit from additional explanation and guidance. Ensure that the help text is clear and directly relates to the fields in the content area. Use images or icons to provide visual cues. Always provide a clear way for users to complete or discard the process. Avoid overwhelming users with too much information; keep both the content and the help text concise.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Button$1("Open Tutorial Modal").OnClick(function (s, e) {
                    Tesserae.Tests.Samples.TutorialModalSample.SampleTutorialModal().Show();
                }), tss.UI.Button$1("Open Large Tutorial Modal").OnClick(function (s, e) {
                    Tesserae.Tests.Samples.TutorialModalSample.SampleTutorialModal().Height(tss.usX.vh$1(90)).Width(tss.usX.vw$1(90)).Show();
                })])).SetTitle("Usage"), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.Button$1("Open Modal Below").OnClick(function (s, e) {
                    container.Content$1(Tesserae.Tests.Samples.TutorialModalSample.SampleTutorialModal().Border("#ffaf66", tss.usX.px$1(5)).ShowEmbedded());
                }), container])).SetTitle("Embedded Modal")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.UIconsSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        statics: {
            methods: {
                ToValidName: function (icon) {
                    var words = System.Linq.Enumerable.from(System.String.split(icon, System.Array.init([45], System.Char).map(function (i) {{ return String.fromCharCode(i); }}), null, 1), System.String).select(function (i) {
                            return (i.substr(0, 1).toUpperCase() || "") + (i.substr(1) || "");
                        }).ToArray(System.String);

                    var name = (words).join("");

                    if (System.Char.isDigit(name.charCodeAt(0))) {
                        return "_" + (name || "");
                    } else {
                        return name;
                    }
                }
            }
        },
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                var $t;
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack().Secondary()), Tesserae.Tests.Samples.UIconsSample, 3460, "A utility to display icons"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("UIcons provide a massive collection of high-quality icons integrated directly into Tesserae. They are accessible through a strongly-typed enum, offering full IntelliSense support and ensuring that your application's iconography is consistent and easily maintainable.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use icons to provide visual context and improve the scanability of your UI. Choose icons that are widely recognized and relevant to the action or content they represent. Maintain consistency in icon style and weight throughout your application. Use the SearchableList below to explore the thousands of available icons.")])).SetTitle("Best Practices")])), tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle(System.String.format("Strongly-typed {0} enum", ["UIcons"])), tss.UI.SearchableList(Tesserae.Tests.Samples.UIconsSample.IconItem, ($t = Tesserae.Tests.Samples.UIconsSample.IconItem, System.Linq.Enumerable.from(this.GetAllIcons(), $t).ToArray($t)), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)])])).SetTitle("Usage")])), true, false, "");
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetAllIcons: function () {
                return new (H5.GeneratorEnumerable$1(Tesserae.Tests.Samples.UIconsSample.IconItem))(H5.fn.bind(this, function ()  {
                    var $s = 0,
                        $jff,
                        $rv,
                        names,
                        values,
                        i,
                        $ae;

                    var $en = new (H5.GeneratorEnumerator$1(Tesserae.Tests.Samples.UIconsSample.IconItem))(H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                switch ($s) {
                                    case 0: {
                                        names = System.Enum.getNames(Tesserae.UIcons);
                                            values = H5.cast(System.Enum.getValues(Tesserae.UIcons), System.Array.type(Tesserae.UIcons));

                                            i = 0;
                                            $s = 1;
                                            continue;
                                    }
                                    case 1: {
                                        if ( i < values.length ) {
                                                $s = 2;
                                                continue;
                                            }
                                        $s = 5;
                                        continue;
                                    }
                                    case 2: {
                                        $en.current = new Tesserae.Tests.Samples.UIconsSample.IconItem(values[System.Array.index(i, values)], names[System.Array.index(i, names)]);
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $s = 4;
                                        continue;
                                    }
                                    case 4: {
                                        i = (i + 1) | 0;
                                        $s = 1;
                                        continue;
                                    }
                                    case 5: {

                                    }
                                    default: {
                                        return false;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            throw $ae;
                        }
                    }));
                    return $en;
                }));
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.UptimeSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var barsItems = new (System.Collections.Generic.List$1(System.ValueTuple$2(Tesserae.UptimeStatus,tss.IC))).ctor();
                for (var i = 0; i < 90; i = (i + 1) | 0) {
                    var status = this.GetRandomStatus();
                    barsItems.add(new (System.ValueTuple$2(Tesserae.UptimeStatus,tss.IC)).$ctor1(status, this.GetTooltip(status, i)));
                }

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.Title(tss.UI.SectionStack().Secondary(), tss.ITFX.Bold(tss.txt, tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock("Uptime")))), tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Displays system status over time using colored segments and month grids."))])).SetTitle("Overview")), tss.UI.Card(tss.UI.UptimeBars().Items(barsItems)).SetTitle("Last 90 days uptime")), tss.UI.Card(tss.UI.UptimeBars().Compact().Items(barsItems)).SetTitle("Service Uptime (compact view)")), tss.UI.Card(tss.ICTX.Children$6(tss.Grid, tss.UI.Grid([tss.usX.fr$1(1), tss.usX.fr$1(1), tss.usX.fr$1(1)]).Gap(tss.usX.px$1(16)), [tss.UI.UptimeCalendar("July 2024", "99.8%").Items(this.GetCalendarItems(90)), tss.UI.UptimeCalendar("August 2024", "98.1%").Items(this.GetCalendarItems(60)), tss.UI.UptimeCalendar("September 2024", "100%").Items(this.GetCalendarItems(30))])).SetTitle("Service Uptime History"));
            }
        },
        methods: {
            GetCalendarItems: function (startDaysAgo) {
                var calItems = new (System.Collections.Generic.List$1(System.ValueTuple$2(Tesserae.UptimeStatus,tss.IC))).ctor();
                for (var i = 0; i < 30; i = (i + 1) | 0) {
                    var status = this.GetRandomStatus();
                    calItems.add(new (System.ValueTuple$2(Tesserae.UptimeStatus,tss.IC)).$ctor1(status, this.GetTooltip(status, ((startDaysAgo - i) | 0))));
                }
                for (var i1 = 0; i1 < 5; i1 = (i1 + 1) | 0) {
                    calItems.add(new (System.ValueTuple$2(Tesserae.UptimeStatus,tss.IC)).$ctor1("future", null));
                }
                return calItems;
            },
            GetRandomStatus: function () {
                var r = Math.random();
                if (r > 0.95) {
                    return "major";
                }
                if (r > 0.9) {
                    return "minor";
                }
                if (r > 0.85) {
                    return "maintenance";
                }
                return "operational";
            },
            GetTooltip: function (status, daysAgo) {
                var date = System.DateTime.format(System.DateTime.addDays(System.DateTime.getToday(), ((-daysAgo) | 0)), "d");
                return tss.UI.Raw$2(tss.UI.Div(tss.UI._$1("tss-uptime-tooltip-content"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(date)), tss.ITFX.Small(tss.txt, tss.UI.TextBlock(System.Enum.toString(Tesserae.UptimeStatus, status)))]).Render()));
            },
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.ValidatorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var looksValidSoFar = tss.UI.TextBlock("?");
                var validator = tss.UI.Validator().OnValidation(function (validity) {
                    looksValidSoFar.Text = validity === tss.VS.Invalid ? "Something is not ok \u274c" : "Everything is fine so far \u2714";
                });

                var textBoxThatMustBeNonEmpty = tss.UI.TextBox$1("").Required();
                var textBoxThatMustBePositiveInteger = tss.UI.TextBox$1("").Required();
                tss.vX.Validation(tss.TextBox, textBoxThatMustBeNonEmpty, function (tb) {
                    return tb.Text.length === 0 ? "must enter a value" : H5.rE(textBoxThatMustBeNonEmpty.Text, textBoxThatMustBePositiveInteger.Text) ? "duplicated  values" : null;
                }, validator);
                tss.vX.Validation(tss.TextBox, textBoxThatMustBePositiveInteger, function (tb) {
                    var $t;
                    return ($t = tss.Validation.NonZeroPositiveInteger(tb), $t != null ? $t : (H5.rE(textBoxThatMustBeNonEmpty.Text, textBoxThatMustBePositiveInteger.Text) ? "duplicated values" : null));
                }, validator);


                var dropdown = tss.vX.Validation(tss.Dropdown, tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1(""), tss.UI.DropdownItem$1("Item 1"), tss.UI.DropdownItem$1("Item 2")]).Required(), function (dd) {
                    return System.String.isNullOrWhiteSpace(dd.SelectedText) ? "must select an item" : null;
                }, validator);

                this.content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.ValidatorSample, 964, "A utility to validate inputs"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Validator is a utility component that aggregates the validation state of multiple UI components. It provides a centralized way to monitor whether a form or a set of inputs is valid, making it easy to provide real-time feedback to users and prevent the submission of incorrect data.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Register all related input components with a single Validator. Use clear and descriptive validation messages that help users correct errors. Avoid showing validation errors immediately on form load; instead, allow users to interact with the fields first. Use the Validator's state to enable or disable primary actions like 'Submit' or 'Save' to ensure only valid data is processed.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic TextBox")), tss.ICTX.Children$6(tss.S, tss.ICX.Padding(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), tss.usX.px$1(8)), [tss.UI.Label$1("Non-empty").SetContent(textBoxThatMustBeNonEmpty), tss.UI.Label$1("Integer > 0 (must not match the value above)").SetContent(textBoxThatMustBePositiveInteger), tss.UI.Label$1("Pre-filled Integer > 0 (initially valid)").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("123").Required(), tss.Validation.NonZeroPositiveInteger, validator)), tss.UI.Label$1("Pre-filled Integer > 0 (initially i  nvalid)").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("xyz").Required(), tss.Validation.NonZeroPositiveInteger, validator)), tss.UI.Label$1("Not empty with forced instant validation").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("").Required(), function (tb) {
                    return System.String.isNullOrWhiteSpace(tb.Text) ? "Can't be empty" : null;
                }, validator, true)), tss.UI.Label$1("Please select something").SetContent(dropdown)]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Results Summary")), tss.ICTX.Children$6(tss.S, tss.ICX.Padding(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), tss.usX.px$1(8)), [tss.UI.Label$1("Validity (this only checks fields that User has interacted with so far)").Inline().SetContent(looksValidSoFar), tss.UI.Label$1("Test revalidation (will fail if repeated)").SetContent(tss.UI.Button$1("Validate").OnClick(function (s, b) {
                    validator.Revalidate();
                }))])])).SetTitle("Usage")]));

                console.log("Is form initially in a valid state: " + System.Boolean.toString(validator.AreCurrentValuesAllValid()));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.VirtualizedListSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.VirtualizedListSample, 2773, "A list that renders its items virtually"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("VirtualizedList is a high-performance component designed for rendering massive datasets\u2014thousands or even tens of thousands of items\u2014without sacrificing UI responsiveness."), tss.UI.TextBlock("It achieves this by only rendering the items that are currently visible within the viewport (plus a small buffer), significantly reducing the number of DOM elements the browser needs to manage.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use VirtualizedList for any list that could potentially contain more than a few hundred items. Ensure that each item has a consistent height for accurate scroll position calculation. Virtualization is most effective when item components are relatively complex or resource-intensive to render. Always provide a clear 'Empty Message' if the dataset is expected to be empty.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Virtualized List with 5,000 Items"), tss.UI.TextBlock("Scroll rapidly to see how the list handles a large number of items."), tss.ICX.MB(tss.VirtualizedList, tss.ICX.Height(tss.VirtualizedList, tss.UI.VirtualizedList().WithListItems(this.GetALotOfItems()), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Empty State"), tss.ICX.Height(tss.VirtualizedList, tss.UI.VirtualizedList().WithEmptyMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.UI.TextBlock("No items available"))))), tss.usX.px$1(100));
                }).WithListItems(System.Linq.Enumerable.empty()), tss.usX.px$1(150))])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetALotOfItems: function () {
                return System.Linq.Enumerable.range(1, 5000).select(function (n) {
                    return new Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem(System.String.format("Virtualized Item {0}", [H5.box(n, System.Int32)]));
                });
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.WeekPickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var minWeek = new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), 1);
                var maxWeek = new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), 52);

                this._content = tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(tss.SectionStackX.FlatSection(Tesserae.Tests.Samples.SamplesHelper.SampleTitle$1(tss.UI.SectionStack().Secondary(), Tesserae.Tests.Samples.WeekPickerSample, 795, "A control to select an ISO week number"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("WeekPicker lets users choose a specific ISO week within a year using the browser's native week-input widget. It surfaces the selection as a typed (year, weekNumber) tuple."), tss.UI.TextBlock("It is well-suited for scheduling, sprint planning, payroll cycles, or any reporting context that aligns to week boundaries rather than individual days or months.")])).SetTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.UI.TextBlock("Use WeekPicker when your domain explicitly talks about calendar weeks (e.g. 'Week 23 of 2025'). Avoid using it as a substitute for a date range picker \u2014 if the user ultimately needs a start and end date, use DatePicker pairs instead. Apply Min/Max constraints to prevent selection of weeks outside the valid planning horizon.")])).SetTitle("Best Practices")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic WeekPicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Week 1 of This Year").SetContent(new tss.WeekPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), 1)).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}-W{1}", H5.box(s.Week.Item1, System.Int32), H5.box(s.Week.Item2, System.Int32)));
                })), tss.UI.Label$1("No Default").SetContent(new tss.WeekPicker(null).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}-W{1}", H5.box(s.Week.Item1, System.Int32), H5.box(s.Week.Item2, System.Int32)));
                })), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(new tss.WeekPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), 1)).Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Min / Max Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1(System.String.format("Range: {0}-W{1} to {2}-W{3}", H5.box(minWeek.Item1, System.Int32), H5.box(minWeek.Item2, System.Int32), H5.box(maxWeek.Item1, System.Int32), H5.box(maxWeek.Item2, System.Int32))).SetContent(new tss.WeekPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), 1)).SetMin(minWeek.$clone()).SetMax(maxWeek.$clone()).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}-W{1}", H5.box(s.Week.Item1, System.Int32), H5.box(s.Week.Item2, System.Int32)));
                }))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), new tss.WeekPicker(new (System.ValueTuple$2(System.Int32,System.Int32)).$ctor1(System.DateTime.getYear(System.DateTime.getToday()), 1)).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Week changed to {0}-W{1}", H5.box(s.Week.Item1, System.Int32), H5.box(s.Week.Item2, System.Int32)));
                })])).SetTitle("Usage")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            }
        }
    });

    var $m = H5.setMetadata,
        $n = ["System","Tesserae","System.Threading.Tasks","Tesserae.Tests.Samples","System.Collections.Generic"];
    $m("Tesserae.Tests.App", function () { return {"att":1048960,"a":4,"s":true,"m":[{"a":1,"n":"Main","is":true,"t":8,"sn":"Main","rt":$n[0].Void},{"a":1,"n":"_sidebarOpenStateKey","is":true,"t":4,"rt":$n[0].String,"sn":"_sidebarOpenStateKey"},{"a":1,"n":"_sidebarOrderKey","is":true,"t":4,"rt":$n[0].String,"sn":"_sidebarOrderKey"}]}; }, $n);
    $m("Tesserae.Tests.SamplesSourceCode", function () { return {"att":1048576,"a":4,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"GetCodeForSample","is":true,"t":8,"pi":[{"n":"sampleName","pt":$n[0].String,"ps":0}],"sn":"GetCodeForSample","rt":$n[0].String,"p":[$n[0].String]}]}; }, $n);
    $m("Tesserae.Tests.ISample", function () { return {"att":1048737,"a":2}; }, $n);
    $m("Tesserae.Tests.Sample", function () { return {"att":1048576,"a":4,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String,$n[0].String,$n[0].Int32,$n[1].UIcons,Function],"pi":[{"n":"type","pt":$n[0].String,"ps":0},{"n":"name","pt":$n[0].String,"ps":1},{"n":"group","pt":$n[0].String,"ps":2},{"n":"order","pt":$n[0].Int32,"ps":3},{"n":"icon","pt":$n[1].UIcons,"ps":4},{"n":"contentGenerator","pt":Function,"ps":5}],"sn":"ctor"},{"a":2,"n":"FormatSampleName","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].String,"ps":0}],"sn":"FormatSampleName","rt":$n[0].String,"p":[$n[0].String]},{"a":2,"n":"FormatSampleName","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].Type,"ps":0}],"sn":"FormatSampleName$1","rt":$n[0].String,"p":[$n[0].Type]},{"a":2,"n":"ContentGenerator","t":16,"rt":Function,"g":{"a":2,"n":"get_ContentGenerator","t":8,"rt":Function,"fg":"ContentGenerator"},"fn":"ContentGenerator"},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"fn":"Group"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"fn":"Icon"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":2,"n":"Order","t":16,"rt":$n[0].Int32,"g":{"a":2,"n":"get_Order","t":8,"rt":$n[0].Int32,"fg":"Order","box":function ($v) { return H5.box($v, System.Int32);}},"fn":"Order"},{"a":2,"n":"Type","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Type","t":8,"rt":$n[0].String,"fg":"Type"},"fn":"Type"},{"a":1,"backing":true,"n":"<ContentGenerator>k__BackingField","t":4,"rt":Function,"sn":"ContentGenerator"},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"},{"a":1,"backing":true,"n":"<Order>k__BackingField","t":4,"rt":$n[0].Int32,"sn":"Order","box":function ($v) { return H5.box($v, System.Int32);}},{"a":1,"backing":true,"n":"<Type>k__BackingField","t":4,"rt":$n[0].String,"sn":"Type"}]}; }, $n);
    $m("Tesserae.Tests.SampleDetailsAttribute", function () { return {"att":1048577,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"s":{"a":2,"n":"set_Group","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Group"},"fn":"Group"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"s":{"a":2,"n":"set_Icon","t":8,"p":[$n[1].UIcons],"rt":$n[0].Void,"fs":"Icon"},"fn":"Icon"},{"a":2,"n":"Order","t":16,"rt":$n[0].Int32,"g":{"a":2,"n":"get_Order","t":8,"rt":$n[0].Int32,"fg":"Order","box":function ($v) { return H5.box($v, System.Int32);}},"s":{"a":2,"n":"set_Order","t":8,"p":[$n[0].Int32],"rt":$n[0].Void,"fs":"Order"},"fn":"Order"},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Order>k__BackingField","t":4,"rt":$n[0].Int32,"sn":"Order","box":function ($v) { return H5.box($v, System.Int32);}}]}; }, $n);
    $m("Tesserae.Tests.Samples.BreadcrumbSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 4445
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetComponentDetailsListItems","t":8,"sn":"GetComponentDetailsListItems","rt":System.Array.type(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents)},{"a":1,"n":"GetDetailsListItems","t":8,"pi":[{"n":"start","dv":1,"o":true,"pt":$n[0].Int32,"ps":0},{"n":"count","dv":100,"o":true,"pt":$n[0].Int32,"ps":1}],"sn":"GetDetailsListItems","rt":System.Array.type(Tesserae.Tests.Samples.DetailsListSampleFileItem),"p":[$n[0].Int32,$n[0].Int32]},{"a":1,"n":"GetDetailsListItemsAsync","t":8,"pi":[{"n":"start","dv":1,"o":true,"pt":$n[0].Int32,"ps":0},{"n":"count","dv":100,"o":true,"pt":$n[0].Int32,"ps":1}],"sn":"GetDetailsListItemsAsync","rt":$n[2].Task$1(System.Array.type(Tesserae.Tests.Samples.DetailsListSampleFileItem)),"p":[$n[0].Int32,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSampleFileItem", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].UIcons,$n[0].String,$n[0].DateTime,$n[0].String,$n[0].Double],"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0},{"n":"name","pt":$n[0].String,"ps":1},{"n":"modified","pt":$n[0].DateTime,"ps":2},{"n":"by","pt":$n[0].String,"ps":3},{"n":"size","pt":$n[0].Double,"ps":4}],"sn":"ctor"},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[3].DetailsListSampleFileItem,"ps":0},{"n":"key","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[3].DetailsListSampleFileItem,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"index","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[4].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"cell","pt":Function,"ps":1}],"sn":"Render","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[4].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"DateModified","t":16,"rt":$n[0].DateTime,"g":{"a":2,"n":"get_DateModified","t":8,"rt":$n[0].DateTime,"fg":"DateModified","box":function ($v) { return H5.box($v, System.DateTime, System.DateTime.format);}},"fn":"DateModified"},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"FileIcon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_FileIcon","t":8,"rt":$n[1].UIcons,"fg":"FileIcon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"fn":"FileIcon"},{"a":2,"n":"FileName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_FileName","t":8,"rt":$n[0].String,"fg":"FileName"},"fn":"FileName"},{"a":2,"n":"FileSize","t":16,"rt":$n[0].Double,"g":{"a":2,"n":"get_FileSize","t":8,"rt":$n[0].Double,"fg":"FileSize","box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},"fn":"FileSize"},{"a":2,"n":"ModifiedBy","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ModifiedBy","t":8,"rt":$n[0].String,"fg":"ModifiedBy"},"fn":"ModifiedBy"},{"a":1,"backing":true,"n":"<DateModified>k__BackingField","t":4,"rt":$n[0].DateTime,"sn":"DateModified","box":function ($v) { return H5.box($v, System.DateTime, System.DateTime.format);}},{"a":1,"backing":true,"n":"<FileIcon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"FileIcon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<FileName>k__BackingField","t":4,"rt":$n[0].String,"sn":"FileName"},{"a":1,"backing":true,"n":"<FileSize>k__BackingField","t":4,"rt":$n[0].Double,"sn":"FileSize","box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},{"a":1,"backing":true,"n":"<ModifiedBy>k__BackingField","t":4,"rt":$n[0].String,"sn":"ModifiedBy"}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSampleItemWithComponents", function () { return {"att":1048577,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[3].DetailsListSampleItemWithComponents,"ps":0},{"n":"key","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[3].DetailsListSampleItemWithComponents,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"index","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[4].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"cell","pt":Function,"ps":1}],"sn":"Render","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[4].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"WithButton","t":8,"pi":[{"n":"btn","pt":tss.Button,"ps":0}],"sn":"WithButton","rt":$n[3].DetailsListSampleItemWithComponents,"p":[tss.Button]},{"a":2,"n":"WithCheckBox","t":8,"pi":[{"n":"cb","pt":tss.ChecBox,"ps":0}],"sn":"WithCheckBox","rt":$n[3].DetailsListSampleItemWithComponents,"p":[tss.ChecBox]},{"a":2,"n":"WithChoiceGroup","t":8,"pi":[{"n":"cg","pt":tss.ChoiceGroup,"ps":0}],"sn":"WithChoiceGroup","rt":$n[3].DetailsListSampleItemWithComponents,"p":[tss.ChoiceGroup]},{"a":2,"n":"WithIcon","t":8,"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0}],"sn":"WithIcon","rt":$n[3].DetailsListSampleItemWithComponents,"p":[$n[1].UIcons]},{"a":2,"n":"WithName","t":8,"pi":[{"n":"name","pt":$n[0].String,"ps":0}],"sn":"WithName","rt":$n[3].DetailsListSampleItemWithComponents,"p":[$n[0].String]},{"a":2,"n":"Button","t":16,"rt":tss.Button,"g":{"a":2,"n":"get_Button","t":8,"rt":tss.Button,"fg":"Button"},"s":{"a":1,"n":"set_Button","t":8,"p":[tss.Button],"rt":$n[0].Void,"fs":"Button"},"fn":"Button"},{"a":2,"n":"CheckBox","t":16,"rt":tss.ChecBox,"g":{"a":2,"n":"get_CheckBox","t":8,"rt":tss.ChecBox,"fg":"CheckBox"},"s":{"a":1,"n":"set_CheckBox","t":8,"p":[tss.ChecBox],"rt":$n[0].Void,"fs":"CheckBox"},"fn":"CheckBox"},{"a":2,"n":"ChoiceGroup","t":16,"rt":tss.ChoiceGroup,"g":{"a":2,"n":"get_ChoiceGroup","t":8,"rt":tss.ChoiceGroup,"fg":"ChoiceGroup"},"s":{"a":1,"n":"set_ChoiceGroup","t":8,"p":[tss.ChoiceGroup],"rt":$n[0].Void,"fs":"ChoiceGroup"},"fn":"ChoiceGroup"},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"s":{"a":1,"n":"set_Icon","t":8,"p":[$n[1].UIcons],"rt":$n[0].Void,"fs":"Icon"},"fn":"Icon"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"s":{"a":1,"n":"set_Name","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Name"},"fn":"Name"},{"a":1,"backing":true,"n":"<Button>k__BackingField","t":4,"rt":tss.Button,"sn":"Button"},{"a":1,"backing":true,"n":"<CheckBox>k__BackingField","t":4,"rt":tss.ChecBox,"sn":"CheckBox"},{"a":1,"backing":true,"n":"<ChoiceGroup>k__BackingField","t":4,"rt":tss.ChoiceGroup,"sn":"ChoiceGroup"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.InfiniteScrollingListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2519
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"page","dv":-1,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"txt","dv":"","o":true,"pt":$n[0].String,"ps":2}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32,$n[0].Int32,$n[0].String]},{"a":1,"n":"GetSomeItemsAsync","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"page","dv":-1,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"txt","dv":"","o":true,"pt":$n[0].String,"ps":2}],"sn":"GetSomeItemsAsync","rt":$n[2].Task$1(System.Array.type(tss.IC)),"p":[$n[0].Int32,$n[0].Int32,$n[0].String]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ItemsListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2773
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"suffix","dv":"","o":true,"pt":$n[0].String,"ps":1}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32,$n[0].String]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.MasonrySample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 2183
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetCards","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetCards","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ObservableStackSample", function () { return {"nested":[$n[3].ObservableStackSample.StackElement],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 3842
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"AddItem","t":8,"sn":"AddItem","rt":$n[0].Void},{"a":1,"n":"Move","t":8,"pi":[{"n":"oldIdx","pt":$n[0].Int32,"ps":0},{"n":"newIdx","pt":$n[0].Int32,"ps":1}],"sn":"Move","rt":$n[0].Void,"p":[$n[0].Int32,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"Update","t":8,"pi":[{"n":"idx","pt":$n[0].Int32,"ps":0},{"n":"item","pt":$n[3].ObservableStackSample.StackElement,"ps":1}],"sn":"Update","rt":$n[0].Void,"p":[$n[0].Int32,$n[3].ObservableStackSample.StackElement]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true},{"a":1,"n":"_elementIndex","is":true,"t":4,"rt":$n[0].Int32,"sn":"_elementIndex","box":function ($v) { return H5.box($v, System.Int32);}},{"a":1,"n":"_stackElementsList","is":true,"t":4,"rt":tss.ObservableList(tss.ICID),"sn":"_stackElementsList"}]}; }, $n);
    $m("Tesserae.Tests.Samples.ObservableStackSample.StackElement", function () { return {"td":$n[3].ObservableStackSample,"att":1048578,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String],"pi":[{"n":"id","pt":$n[0].String,"ps":0},{"n":"displayName","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":2,"n":"Color","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Color","t":8,"rt":$n[0].String,"fg":"Color"},"fn":"Color"},{"a":2,"n":"ContentHash","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ContentHash","t":8,"rt":$n[0].String,"fg":"ContentHash"},"fn":"ContentHash"},{"a":2,"n":"DisplayName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_DisplayName","t":8,"rt":$n[0].String,"fg":"DisplayName"},"s":{"a":2,"n":"set_DisplayName","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"DisplayName"},"fn":"DisplayName"},{"a":2,"n":"Id","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Id","t":8,"rt":$n[0].String,"fg":"Id"},"s":{"a":2,"n":"set_Id","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Id"},"fn":"Id"},{"a":2,"n":"Identifier","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Identifier","t":8,"rt":$n[0].String,"fg":"Identifier"},"fn":"Identifier"},{"a":1,"backing":true,"n":"<Color>k__BackingField","t":4,"rt":$n[0].String,"sn":"Color"},{"a":1,"backing":true,"n":"<DisplayName>k__BackingField","t":4,"rt":$n[0].String,"sn":"DisplayName"},{"a":1,"backing":true,"n":"<Id>k__BackingField","t":4,"rt":$n[0].String,"sn":"Id"}]}; }, $n);
    $m("Tesserae.Tests.Samples.HashingHelper", function () { return {"att":1048961,"a":2,"s":true,"m":[{"a":2,"n":"Fnv1aHash","is":true,"t":8,"pi":[{"n":"value","pt":$n[0].String,"ps":0}],"sn":"Fnv1aHash","rt":$n[0].Int32,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}}]}; }, $n);
    $m("Tesserae.Tests.Samples.OverflowSetSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 10, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableGroupedListSample", function () { return {"nested":[$n[3].SearchableGroupedListSample.SearchableGroupedListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 3936
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetItems","rt":System.Array.type(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem", function () { return {"td":$n[3].SearchableGroupedListSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String],"pi":[{"n":"value","pt":$n[0].String,"ps":0},{"n":"group","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"fn":"Group"},{"a":1,"n":"_component","t":4,"rt":tss.IC,"sn":"_component","ro":true},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableListSample", function () { return {"nested":[$n[3].SearchableListSample.SearchableListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 3936
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetItems","rt":System.Array.type(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableListSample.SearchableListItem", function () { return {"td":$n[3].SearchableListSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"value","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_component","t":4,"rt":tss.IC,"sn":"_component","ro":true},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.StackSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 3842
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.S,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.S,$n[0].Int32]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true},{"a":1,"n":"sampleSortableStackLocalStorageKey","is":true,"t":4,"rt":$n[0].String,"sn":"sampleSortableStackLocalStorageKey"}]}; }, $n);
    $m("Tesserae.Tests.Samples.TimelineSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 4577
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"BuildTimeline","t":8,"pi":[{"n":"timeline","pt":tss.Timeline,"ps":0}],"sn":"BuildTimeline","rt":tss.Timeline,"p":[tss.Timeline]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.VirtualizedListSample", function () { return {"nested":[$n[3].VirtualizedListSample.SampleVirtualizedItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2773
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetALotOfItems","t":8,"sn":"GetALotOfItems","rt":$n[4].IEnumerable$1(Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem", function () { return {"td":$n[3].VirtualizedListSample,"att":1048834,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_innerElement","t":4,"rt":HTMLElement,"sn":"_innerElement","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.AccordionSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 33
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ActionButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 5, Icon: 1380
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.AnnotatedTextEditorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 22, Icon: 2380
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"AnnotateAllTokensAsync","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"AnnotateAllTokensAsync","rt":$n[2].Task$1(System.Array.type(tss.AnnotatedTextEditor.Entity)),"p":[$n[0].String]},{"a":1,"n":"AnnotateAsync","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"AnnotateAsync","rt":$n[2].Task$1(System.Array.type(tss.AnnotatedTextEditor.Entity)),"p":[$n[0].String]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SampleText","is":true,"t":4,"rt":$n[0].String,"sn":"SampleText"},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true},{"a":1,"n":"_vocabulary","is":true,"t":4,"rt":$n[0].Array.type(System.ValueTuple$5(System.String,System.String,System.String,System.String,System.String)),"sn":"_vocabulary","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.AvatarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 4799
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.BadgeSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 4462
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1378
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CardSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 48
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content"}]}; }, $n);
    $m("Tesserae.Tests.Samples.CarouselSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 3460
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ChatSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 101, Icon: 1222
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CheckBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 969
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ChoiceGroupSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2773
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ColorPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 3262
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CommandBarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CronEditorSample", function () { return {"att":1048833,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 1110
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetObservableExample","t":8,"sn":"GetObservableExample","rt":tss.IC},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DatePickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 768
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DateTimePickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 774
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DeltaComponentSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 3728
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DropdownSample", function () { return {"att":1048833,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 873
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"DeferredDropdownItem","t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0},{"n":"delayMs","pt":$n[0].Int32,"ps":1}],"sn":"DeferredDropdownItem","rt":tss.Dropdown.Item,"p":[$n[0].String,$n[0].Int32]},{"a":1,"n":"GetItemsAsync","t":8,"sn":"GetItemsAsync","rt":$n[2].Task$1(System.Array.type(tss.Dropdown.Item))},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"StartLoadingAsyncDataImmediately","is":true,"t":8,"pi":[{"n":"dropdown","pt":tss.Dropdown,"ps":0}],"sn":"StartLoadingAsyncDataImmediately","rt":tss.Dropdown,"p":[tss.Dropdown]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EditableLabelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 1663
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.GridPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 4441
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetGameOfLifeSample","is":true,"t":8,"sn":"GetGameOfLifeSample","rt":tss.S},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.GridSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 2183
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.HorizontalSeparatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2416
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.LabelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 4524
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.MarkdownBlockSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 3275
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.MessageSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1221
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.MetricSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 25, Icon: 929
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.MonthPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 203, Icon: 782
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NavbarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NumberPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 2525
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.OmniBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 21, Icon: 3938
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PaginationSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 2773
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 873
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetComponentPickerItems","t":8,"sn":"GetComponentPickerItems","rt":System.Array.type(Tesserae.Tests.Samples.PickerSampleItemWithComponents)},{"a":1,"n":"GetPickerItems","t":8,"sn":"GetPickerItems","rt":System.Array.type(Tesserae.Tests.Samples.PickerSampleItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSampleItem", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"name","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"IsSelected","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_IsSelected","t":8,"rt":$n[0].Boolean,"fg":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"s":{"a":2,"n":"set_IsSelected","t":8,"p":[$n[0].Boolean],"rt":$n[0].Void,"fs":"IsSelected"},"fn":"IsSelected"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":1,"backing":true,"n":"<IsSelected>k__BackingField","t":4,"rt":$n[0].Boolean,"sn":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSampleItemWithComponents", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[1].UIcons],"pi":[{"n":"name","pt":$n[0].String,"ps":0},{"n":"icon","pt":$n[1].UIcons,"ps":1}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"IsSelected","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_IsSelected","t":8,"rt":$n[0].Boolean,"fg":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"s":{"a":2,"n":"set_IsSelected","t":8,"p":[$n[0].Boolean],"rt":$n[0].Void,"fs":"IsSelected"},"fn":"IsSelected"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":1,"n":"_icon","t":4,"rt":$n[1].UIcons,"sn":"_icon","ro":true,"box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<IsSelected>k__BackingField","t":4,"rt":$n[0].Boolean,"sn":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.PlanSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 2774
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.RatingSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 50, Icon: 4324
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ResourceCardSample", function () { return {"nested":[$n[3].ResourceCardSample.ModelItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 2492
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetModelItems","t":8,"sn":"GetModelItems","rt":System.Array.type(Tesserae.Tests.Samples.ResourceCardSample.ModelItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ResourceCardSample.ModelItem", function () { return {"td":$n[3].ResourceCardSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String,$n[0].String,$n[0].String,$n[0].String,$n[1].UIcons],"pi":[{"n":"title","pt":$n[0].String,"ps":0},{"n":"author","pt":$n[0].String,"ps":1},{"n":"capability","pt":$n[0].String,"ps":2},{"n":"description","pt":$n[0].String,"ps":3},{"n":"date","pt":$n[0].String,"ps":4},{"n":"icon","pt":$n[1].UIcons,"ps":5}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"Author","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Author","t":8,"rt":$n[0].String,"fg":"Author"},"fn":"Author"},{"a":2,"n":"Capability","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Capability","t":8,"rt":$n[0].String,"fg":"Capability"},"fn":"Capability"},{"a":2,"n":"Date","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Date","t":8,"rt":$n[0].String,"fg":"Date"},"fn":"Date"},{"a":2,"n":"Description","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Description","t":8,"rt":$n[0].String,"fg":"Description"},"fn":"Description"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"fn":"Icon"},{"a":2,"n":"Title","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Title","t":8,"rt":$n[0].String,"fg":"Title"},"fn":"Title"},{"a":1,"backing":true,"n":"<Author>k__BackingField","t":4,"rt":$n[0].String,"sn":"Author"},{"a":1,"backing":true,"n":"<Capability>k__BackingField","t":4,"rt":$n[0].String,"sn":"Capability"},{"a":1,"backing":true,"n":"<Date>k__BackingField","t":4,"rt":$n[0].String,"sn":"Date"},{"a":1,"backing":true,"n":"<Description>k__BackingField","t":4,"rt":$n[0].String,"sn":"Description"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Title>k__BackingField","t":4,"rt":$n[0].String,"sn":"Title"}]}; }, $n);
    $m("Tesserae.Tests.Samples.SaveButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 1530
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"DynamicTextUpdateSample","t":8,"sn":"DynamicTextUpdateSample","rt":tss.IC},{"a":1,"n":"GetVerifyingWhileButton","t":8,"sn":"GetVerifyingWhileButton","rt":tss.IC},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SavingToastSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 411
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ShowMany","t":8,"sn":"ShowMany","rt":$n[2].Task},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 3936
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SidebarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"CreateDeepNav","is":true,"t":8,"pi":[{"n":"path","pt":$n[0].String,"ps":0},{"n":"currentDepth","dv":0,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"maxDepth","dv":3,"o":true,"pt":$n[0].Int32,"ps":2}],"sn":"CreateDeepNav","rt":$n[4].IEnumerable$1(Tesserae.ISidebarItem),"p":[$n[0].String,$n[0].Int32,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SidebarSeparatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 99, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SidenavSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 101, Icon: 151
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"FillBuildSection","is":true,"t":8,"pi":[{"n":"sidebar","pt":tss.Sidebar,"ps":0}],"sn":"FillBuildSection","rt":$n[0].Void,"p":[tss.Sidebar]},{"a":1,"n":"FillConfigureSection","is":true,"t":8,"pi":[{"n":"sidebar","pt":tss.Sidebar,"ps":0}],"sn":"FillConfigureSection","rt":$n[0].Void,"p":[tss.Sidebar]},{"a":1,"n":"FillGovernSection","is":true,"t":8,"pi":[{"n":"sidebar","pt":tss.Sidebar,"ps":0}],"sn":"FillGovernSection","rt":$n[0].Void,"p":[tss.Sidebar]},{"a":1,"n":"FillHomeSection","is":true,"t":8,"pi":[{"n":"sidebar","pt":tss.Sidebar,"ps":0}],"sn":"FillHomeSection","rt":$n[0].Void,"p":[tss.Sidebar]},{"a":1,"n":"FillOperateSection","is":true,"t":8,"pi":[{"n":"sidebar","pt":tss.Sidebar,"ps":0}],"sn":"FillOperateSection","rt":$n[0].Void,"p":[tss.Sidebar]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SwitchTo","is":true,"t":8,"pi":[{"n":"sidenav","pt":tss.Sidenav,"ps":0},{"n":"sidebar","pt":tss.Sidebar,"ps":1},{"n":"title","pt":$n[1].SidebarText,"ps":2},{"n":"identifier","pt":$n[0].String,"ps":3},{"n":"sectionName","pt":$n[0].String,"ps":4},{"n":"fill","pt":Function,"ps":5}],"sn":"SwitchTo","rt":$n[0].Void,"p":[tss.Sidenav,tss.Sidebar,$n[1].SidebarText,$n[0].String,$n[0].String,Function]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SkeletonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 4226
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SliderSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 3975
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SparklineSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 206, Icon: 931
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SplitViewSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 4440
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.StepperSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 2774
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.StepsSliderSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 202, Icon: 3975
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TaskBoardSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 31, Icon: 1411
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TeachingSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 200, Icon: 4016
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextAreaSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 1, Icon: 4524
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBlockSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1763
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2527
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBreadcrumbsSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TimeHistogramPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 929
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"BucketPickerDemo","is":true,"t":8,"pi":[{"n":"buckets","pt":System.Array.type(tss.TimeHistogramBucket),"ps":0}],"sn":"BucketPickerDemo","rt":tss.IC,"p":[System.Array.type(tss.TimeHistogramBucket)]},{"a":1,"n":"DefaultRenderTime","is":true,"t":8,"pi":[{"n":"time","pt":$n[0].DateTime,"ps":0}],"sn":"DefaultRenderTime","rt":$n[0].String,"p":[$n[0].DateTime]},{"a":1,"n":"FormatSelection","is":true,"t":8,"pi":[{"n":"from","pt":$n[0].DateTime,"ps":0},{"n":"to","pt":$n[0].DateTime,"ps":1},{"n":"count","pt":$n[0].Int32,"ps":2},{"n":"renderTime","dv":null,"o":true,"pt":Function,"ps":3}],"sn":"FormatSelection","rt":$n[0].String,"p":[$n[0].DateTime,$n[0].DateTime,$n[0].Int32,Function]},{"a":1,"n":"GetDailyBackendBuckets","is":true,"t":8,"sn":"GetDailyBackendBuckets","rt":System.Array.type(tss.TimeHistogramBucket)},{"a":1,"n":"GetDenseValues","is":true,"t":8,"sn":"GetDenseValues","rt":$n[0].Array.type(System.DateTime)},{"a":1,"n":"GetFineGrainedValues","is":true,"t":8,"sn":"GetFineGrainedValues","rt":$n[0].Array.type(System.DateTime)},{"a":1,"n":"GetGappedClusterValues","is":true,"t":8,"sn":"GetGappedClusterValues","rt":$n[0].Array.type(System.DateTime)},{"a":1,"n":"GetLargeValues","is":true,"t":8,"sn":"GetLargeValues","rt":$n[0].Array.type(System.DateTime)},{"a":1,"n":"GetLongRangeBackendBuckets","is":true,"t":8,"sn":"GetLongRangeBackendBuckets","rt":System.Array.type(tss.TimeHistogramBucket)},{"a":1,"n":"GetMultiYearValues","is":true,"t":8,"sn":"GetMultiYearValues","rt":$n[0].Array.type(System.DateTime)},{"a":1,"n":"GetSmallValues","is":true,"t":8,"sn":"GetSmallValues","rt":$n[0].Array.type(System.DateTime)},{"a":1,"n":"PickerDemo","is":true,"t":8,"pi":[{"n":"values","pt":$n[0].Array.type(System.DateTime),"ps":0},{"n":"maxBuckets","pt":$n[0].Int32,"ps":1},{"n":"renderTime","dv":null,"o":true,"pt":Function,"ps":2},{"n":"showBucketTooltipOnHover","dv":true,"o":true,"pt":$n[0].Boolean,"ps":3}],"sn":"PickerDemo","rt":tss.IC,"p":[$n[0].Array.type(System.DateTime),$n[0].Int32,Function,$n[0].Boolean]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TimePickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 205, Icon: 1110
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ToggleSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 3975
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ToolCallSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 102, Icon: 4615
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TreeSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 2023
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.UptimeSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 773
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetCalendarItems","t":8,"pi":[{"n":"startDaysAgo","pt":$n[0].Int32,"ps":0}],"sn":"GetCalendarItems","rt":$n[4].IEnumerable$1(System.ValueTuple$2(Tesserae.UptimeStatus,tss.IC)),"p":[$n[0].Int32]},{"a":1,"n":"GetRandomStatus","t":8,"sn":"GetRandomStatus","rt":$n[1].UptimeStatus,"box":function ($v) { return H5.box($v, Tesserae.UptimeStatus, System.Enum.toStringFn(Tesserae.UptimeStatus));}},{"a":1,"n":"GetTooltip","t":8,"pi":[{"n":"status","pt":$n[1].UptimeStatus,"ps":0},{"n":"daysAgo","pt":$n[0].Int32,"ps":1}],"sn":"GetTooltip","rt":tss.IC,"p":[$n[1].UptimeStatus,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.WeekPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 204, Icon: 795
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ProgressIndicatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 10, Icon: 363
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ProgressModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 20, Icon: 5061
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ProgressRingSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 10, Icon: 1014
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SpinnerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 10, Icon: 4226
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SamplesHelper", function () { return {"att":1048961,"a":2,"s":true,"m":[{"a":2,"n":"SampleDo","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleDo","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleDont","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleDont","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleSubTitle","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleSubTitle","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleTitle","is":true,"t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"sampleType","pt":$n[0].String,"ps":1},{"n":"icon","pt":$n[1].UIcons,"ps":2},{"n":"subtitle","pt":$n[0].String,"ps":3}],"sn":"SampleTitle","rt":tss.SectionStack,"p":[tss.SectionStack,$n[0].String,$n[1].UIcons,$n[0].String]},{"a":2,"n":"SampleTitle","is":true,"t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"sampleType","pt":$n[0].Type,"ps":1},{"n":"icon","pt":$n[1].UIcons,"ps":2},{"n":"subtitle","pt":$n[0].String,"ps":3}],"sn":"SampleTitle$1","rt":tss.SectionStack,"p":[tss.SectionStack,$n[0].Type,$n[1].UIcons,$n[0].String]},{"a":2,"n":"ShowSampleCode","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].String,"ps":0}],"sn":"ShowSampleCode","rt":$n[0].Void,"p":[$n[0].String]}]}; }, $n);
    $m("Tesserae.Tests.Samples.CardPivotSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 22, Icon: 1330
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ContextMenuSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 152
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DialogSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 5062
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.FloatSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 0, Icon: 2082
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.HorizontalSplitViewSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 207, Icon: 4230
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.LayerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 0, Icon: 4444
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 5063
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PanelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 5062
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PivotSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 4441
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetManyTabsPivot","t":8,"sn":"GetManyTabsPivot","rt":tss.Pivot},{"a":1,"n":"GetPivot","t":8,"sn":"GetPivot","rt":tss.Pivot},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PivotSelectorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 4441
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SectionStackSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 541
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.SectionStack,$n[0].Int32]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SegmentedPivotSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 23, Icon: 2944
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TabbedModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 25, Icon: 686
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"AddModalTab","t":8,"pi":[{"n":"closeable","pt":$n[0].Boolean,"ps":0}],"sn":"AddModalTab","rt":$n[0].Void,"p":[$n[0].Boolean]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_modalCount","t":4,"rt":$n[0].Int32,"sn":"_modalCount","box":function ($v) { return H5.box($v, System.Int32);}},{"a":1,"n":"_pivot","t":4,"rt":tss.Pivot,"sn":"_pivot"},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TutorialModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 2512
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SampleTutorialModal","is":true,"t":8,"sn":"SampleTutorialModal","rt":tss.TutorialModal},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ColorPaletteSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3262
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ColorsSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3262
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetGroupName","t":8,"pi":[{"n":"color","pt":$n[0].ValueTuple$2(System.String,System.String),"ps":0}],"sn":"GetGroupName","rt":$n[0].String,"p":[$n[0].ValueTuple$2(System.String,System.String)]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"RenderColorStack","t":8,"pi":[{"n":"colorName","pt":$n[0].String,"ps":0},{"n":"colorVar","pt":$n[0].String,"ps":1}],"sn":"RenderColorStack","rt":tss.IC,"p":[$n[0].String,$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CommandPaletteSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 31, Icon: 1220
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"BuildActions","is":true,"t":8,"sn":"BuildActions","rt":$n[4].IEnumerable$1(tss.CommandPaletteAction)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DeferSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 4226
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.SectionStack,$n[0].Int32]},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DeferWithProgressSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 4226
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EmojiSample", function () { return {"nested":[$n[3].EmojiSample.IconItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 4133
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetAllIcons","t":8,"sn":"GetAllIcons","rt":$n[4].IEnumerable$1(Tesserae.Tests.Samples.EmojiSample.IconItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ToValidName","is":true,"t":8,"pi":[{"n":"icon","pt":$n[0].String,"ps":0}],"sn":"ToValidName","rt":$n[0].String,"p":[$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EmojiSample.IconItem", function () { return {"td":$n[3].EmojiSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].Emoji,$n[0].String],"pi":[{"n":"icon","pt":$n[1].Emoji,"ps":0},{"n":"name","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"n":"component","t":4,"rt":tss.IC,"sn":"component","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.FileSelectorAndDropAreaSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 20, Icon: 2004
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.GradientsSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 11, Icon: 3262
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"RenderGradientStack","t":8,"pi":[{"n":"gradientName","pt":$n[0].String,"ps":0},{"n":"gradientVar","pt":$n[0].String,"ps":1}],"sn":"RenderGradientStack","rt":tss.IC,"p":[$n[0].String,$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.KeyboardShortcutSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 11, Icon: 2612
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NodeViewSample", function () { return {"nested":[$n[3].NodeViewSample.HelloWorldNode,$n[3].NodeViewSample.ComplexNode,$n[3].NodeViewSample.DynamicNode],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 3123
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NodeViewSample.HelloWorldNode", function () { return {"td":$n[3].NodeViewSample,"att":1048578,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Inputs","t":16,"rt":System.Array.type(Tesserae.INodeInput),"g":{"a":2,"n":"get_Inputs","t":8,"rt":System.Array.type(Tesserae.INodeInput),"fg":"Inputs"},"fn":"Inputs"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":2,"n":"Outputs","t":16,"rt":System.Array.type(Tesserae.INodeOutput),"g":{"a":2,"n":"get_Outputs","t":8,"rt":System.Array.type(Tesserae.INodeOutput),"fg":"Outputs"},"fn":"Outputs"}]}; }, $n);
    $m("Tesserae.Tests.Samples.NodeViewSample.ComplexNode", function () { return {"td":$n[3].NodeViewSample,"att":1048578,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Inputs","t":16,"rt":System.Array.type(Tesserae.INodeInput),"g":{"a":2,"n":"get_Inputs","t":8,"rt":System.Array.type(Tesserae.INodeInput),"fg":"Inputs"},"fn":"Inputs"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":2,"n":"Outputs","t":16,"rt":System.Array.type(Tesserae.INodeOutput),"g":{"a":2,"n":"get_Outputs","t":8,"rt":System.Array.type(Tesserae.INodeOutput),"fg":"Outputs"},"fn":"Outputs"}]}; }, $n);
    $m("Tesserae.Tests.Samples.NodeViewSample.DynamicNode", function () { return {"td":$n[3].NodeViewSample,"att":1048578,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"UpdateNode","t":8,"pi":[{"n":"inputs","pt":System.Object,"ps":0},{"n":"outputs","pt":System.Object,"ps":1},{"n":"addInput","pt":Function,"ps":2},{"n":"addOutput","pt":Function,"ps":3}],"sn":"UpdateNode","rt":$n[0].Void,"p":[System.Object,System.Object,Function,Function]},{"a":2,"n":"Inputs","t":16,"rt":System.Array.type(Tesserae.INodeInput),"g":{"a":2,"n":"get_Inputs","t":8,"rt":System.Array.type(Tesserae.INodeInput),"fg":"Inputs"},"fn":"Inputs"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":2,"n":"Outputs","t":16,"rt":System.Array.type(Tesserae.INodeOutput),"g":{"a":2,"n":"get_Outputs","t":8,"rt":System.Array.type(Tesserae.INodeOutput),"fg":"Outputs"},"fn":"Outputs"}]}; }, $n);
    $m("Tesserae.Tests.Samples.NotificationCenterSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 12, Icon: 411
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SanitizeHTMLSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 30, Icon: 3993
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ThemeColorsSample", function () { return {"nested":[$n[3].ThemeColorsSample.ColorListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3262
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"DumpTheme","is":true,"t":8,"sn":"DumpTheme","rt":$n[0].Void},{"a":2,"n":"LightDiff","is":true,"t":8,"pi":[{"n":"from","pt":$n[0].String,"ps":0},{"n":"to","pt":$n[0].String,"ps":1}],"sn":"LightDiff","rt":$n[0].Double,"p":[$n[0].String,$n[0].String],"box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content"}]}; }, $n);
    $m("Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem", function () { return {"td":$n[3].ThemeColorsSample,"att":1048578,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"themeName","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":1,"n":"ColorSquare","is":true,"t":8,"pi":[{"n":"color","pt":$n[0].String,"ps":0}],"sn":"ColorSquare","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[3].ThemeColorsSample.ColorListItem,"ps":0},{"n":"columnSortingKey","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[3].ThemeColorsSample.ColorListItem,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"listItemIndex","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[4].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"createGridCellExpression","pt":Function,"ps":1}],"sn":"Render","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[4].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"ThemeName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ThemeName","t":8,"rt":$n[0].String,"fg":"ThemeName"},"fn":"ThemeName"},{"a":2,"n":"Mapping","is":true,"t":4,"rt":$n[4].Dictionary$2(System.String,System.Collections.Generic.Dictionary$2(System.String,System.String)),"sn":"Mapping"},{"a":1,"backing":true,"n":"<ThemeName>k__BackingField","t":4,"rt":$n[0].String,"sn":"ThemeName"}]}; }, $n);
    $m("Tesserae.Tests.Samples.TippySample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 1243
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ToastSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 20, Icon: 657
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.UIconsSample", function () { return {"nested":[$n[3].UIconsSample.IconItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3460
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetAllIcons","t":8,"sn":"GetAllIcons","rt":$n[4].IEnumerable$1(Tesserae.Tests.Samples.UIconsSample.IconItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ToValidName","is":true,"t":8,"pi":[{"n":"icon","pt":$n[0].String,"ps":0}],"sn":"ToValidName","rt":$n[0].String,"p":[$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.UIconsSample.IconItem", function () { return {"td":$n[3].UIconsSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].UIcons,$n[0].String],"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0},{"n":"name","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"n":"component","t":4,"rt":tss.IC,"sn":"component","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ValidatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 964
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
});
