/**
 * @compiler H5 26.2.64766+327153054a660419545caf3a08c32595d4aee9b5
 */
H5.assemblyVersion("Tesserae.Tests","2026.3.64861.0");
H5.assembly("Tesserae.Tests", function ($asm, globals) {
    "use strict";

    H5.define("Tesserae.Tests.App", {
        main: function Main () {
            var $t, $t1, $t2, $t3;
            var SelectSidebar = null;
            document.body.style.overflow = "hidden";

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


            sidebar.AddHeader(new Tesserae.SidebarText("header", "Tesserae", "TSS", "tss-fontsize-xlarge", "tss-fontweight-bold").PB(16).PL(12));

            var searchBox = new Tesserae.SidebarSearchBox("search", "Search...");
            searchBox.OnSearch(function (term) {
                sidebar.Search(term);
            });
            sidebar.AddHeader(searchBox);

            var pageContent = tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.HS(tss.Sidebar, sidebar), tss.ICX.Grow(tss.IDefer, tss.ICX.W(tss.IDefer, tss.ICX.HS(tss.IDefer, tss.UI.DeferSync$3(Tesserae.Tests.Sample, currentPage, function (page) {
                return (page == null) ? H5.cast(Tesserae.Tests.App.CenteredCardWithBackground(tss.UI.TextBlock("Select an item")), tss.IC) : tss.ICTX.Children$6(tss.S, tss.ScrollBar.ScrollY(tss.S, tss.ICX.S(tss.S, tss.UI.VStack())), [tss.ICX.WS(tss.IC, page.ContentGenerator())]);
            })), 1))]));

            tss.UI.MountToBody(pageContent);


            var samples = System.Linq.Enumerable.from(H5.Reflection.getAssemblyTypes(H5.Reflection.getTypeAssembly(Tesserae.Tests.ISample)), System.Type).where(function (t) {
                    return H5.Reflection.isAssignableFrom(Tesserae.Tests.ISample, t) && !H5.Reflection.isInterface(t);
                }).select(function (sampleType) {
                var sg = H5.as(System.Linq.Enumerable.from(H5.Reflection.getAttributes(sampleType, Tesserae.Tests.SampleDetailsAttribute, true), System.Object).firstOrDefault(null, null), Tesserae.Tests.SampleDetailsAttribute);
                var group = H5.is(sg, System.Object) ? sg.Group : "Others";
                var order = H5.is(sg, System.Object) ? sg.Order : 0;
                var icon = H5.is(sg, System.Object) ? sg.Icon : 925;
                return new Tesserae.Tests.Sample(H5.Reflection.getTypeName(sampleType), Tesserae.Tests.App.FormatSampleName(sampleType), group, order, icon, function () {
                    return H5.as(H5.createInstance(sampleType), tss.IC);
                });
            }).toDictionary(function (s) {
                    return s.Name;
                }, function (s) {
                    return s;
                }, System.String, Tesserae.Tests.Sample);

            sidebar.AddHeader(new Tesserae.SidebarButton.$ctor2("SOURCE_CODE", 696, "Source Code", [new Tesserae.SidebarCommand.$ctor3(218).Tooltip$1("Open repository on GitHub").OnClick(function () {
                window.open("https://github.com/curiosity-ai/tesserae", "_blank");
            })]).CommandsAlwaysVisible().OnOpenIconClick(function () {
                tss.UI.Toast().Success("You clicked on the icon");
            }));

            var openClose = new Tesserae.SidebarCommand.$ctor3(104).Tooltip$1("Close Sidebar");
            var v = { };

            var sidebarOpenState = System.Boolean.tryParse(localStorage.getItem(Tesserae.Tests.App._sidebarOpenStateKey), v) ? v.v : true;

            sidebar.Closed(!sidebarOpenState);

            openClose.OnClick(function () {
                sidebar.Toggle();

                if (sidebar.IsClosed) {
                    openClose.SetIcon$1(105).Tooltip$1("Open Sidebar");
                    localStorage.setItem(Tesserae.Tests.App._sidebarOpenStateKey, System.Boolean.toString((false)));
                } else {
                    openClose.SetIcon$1(104).Tooltip$1("Close Sidebar");
                    localStorage.setItem(Tesserae.Tests.App._sidebarOpenStateKey, System.Boolean.toString((true)));
                }
            });

            var lightDark = new Tesserae.SidebarCommand.$ctor3(4036).Tooltip$1("Light Mode");

            lightDark.OnClick(function () {
                if (tss.UI.Theme.IsDark) {
                    tss.UI.Theme.Light();
                    lightDark.SetIcon$1(4036).Tooltip$1("Light Mode");
                } else {
                    tss.UI.Theme.Dark();
                    lightDark.SetIcon$1(2808).Tooltip$1("Dark Mode");
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
                            var sidebarItem = new Tesserae.SidebarButton.$ctor6((item.v.Name || "") + H5.identity(itemIndex, ((itemIndex = (itemIndex + 1) | 0))), item.v.Icon, item.v.Name, [new Tesserae.SidebarCommand.$ctor3(3948).Tooltip$1("Show sample code").OnClick((function ($me, item) {
                                return function () {
                                    Tesserae.Tests.Samples.SamplesHelper.ShowSampleCode(item.v.Type);
                                };
                            })(this, item)), new Tesserae.SidebarCommand.$ctor3(218).Tooltip$1("Open in new tab").OnClick((function ($me, item) {
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
            },
            methods: {
                FormatSampleName: function (sampleType) {
                    return System.String.replaceAll(System.String.replaceAll(H5.toArray(System.Linq.Enumerable.from(System.String.replaceAll(H5.Reflection.getTypeName(sampleType), "Sample", ""), System.Char).select(function (c) {
                                return H5.isUpper(c) ? " " + String.fromCharCode(c) : "" + String.fromCharCode(c);
                            })).join("").trim(), "U Icons", "UIcons"), " And ", " and ");
                },
                CenteredCardWithBackground: function (content) {
                    var card = tss.ICX.Padding(tss.Card, tss.UI.Card(content).NoAnimation(), tss.usX.px$1(32));
                    card.Render().style.maxHeight = "calc(100% - 32px)";
                    return tss.ICX.S(tss.BackgroundArea, tss.UI.BackgroundArea(card));
                }
            }
        }
    });

    H5.define("Tesserae.Tests.ISample", {
        $kind: "interface"
    });

    H5.define("Tesserae.Tests.Sample", {
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

    H5.define("Tesserae.Tests.Samples.SamplesHelper", {
        statics: {
            methods: {
                SampleHeader: function (sampleType) {
                    var text = System.String.replaceAll(sampleType, "Sample", "");

                    return tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack().Horizontal()), [tss.ITFX.Bold(tss.txt, tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock(text))), tss.ICX.Grow(tss.Raw, tss.UI.Raw$1(), 1), tss.UI.Button$1().SetIcon$1(3948).SetTitle("View code for this sample").OnClick$1(function () {
                        Tesserae.Tests.Samples.SamplesHelper.ShowSampleCode(sampleType);
                    })]);
                },
                ShowSampleCode: function (sampleType) {
                    var text = System.String.replaceAll(sampleType, "Sample", "");
                    tss.LayerExtensions.Content(tss.Modal, tss.ICX.W$1(tss.Modal, tss.UI.Modal((text || "") + " sample code").LightDismiss(), tss.usX.vh$1(80)).ShowCloseButton(), tss.ICX.H$1(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1(Tesserae.Tests.SamplesSourceCode.GetCodeForSample(sampleType))), tss.usX.vh$1(80))).Show();
                },
                SampleTitle: function (text) {
                    return tss.ICX.PB(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(text))), 16);
                },
                SampleSubTitle: function (text) {
                    return tss.ICX.PB(tss.txt, tss.ITFX.Medium(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(text))), 16);
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
                this._component = tss.UI.Card(tss.UI.TextBlock(value));
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
                this._component = tss.UI.Card(tss.UI.TextBlock(value));
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
                        case "BreadcrumbSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 20, Icon = UIcons.MenuDots)]\r\n    public class BreadcrumbSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public BreadcrumbSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(BreadcrumbSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Breadcrumbs provide a secondary navigation system that reveals a user's location in a website or web app. They allow for one-click access to any higher level in the hierarchy.\"),\r\n                    TextBlock(\"Unlike TextBreadcrumbs, this component supports more advanced configuration like custom chevrons, overflow indices, and different sizes.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Place breadcrumbs at the top of the page, above the primary content. Use them when the site hierarchy is at least two levels deep. Each breadcrumb item should represent a page or a container. The last item should represent the current location and be non-clickable. Ensure that the breadcrumbs collapse gracefully on smaller screens or when space is limited.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Breadcrumbs\"),\r\n                    Breadcrumb().Items(\r\n                        Crumb(\"Home\").OnClick((s, e) => Toast().Information(\"Home\")),\r\n                        Crumb(\"Project A\").OnClick((s, e) => Toast().Information(\"Project A\")),\r\n                        Crumb(\"Subfolder 1\").OnClick((s, e) => Toast().Information(\"Subfolder 1\")),\r\n                        Crumb(\"Current Page\")\r\n                    ).PB(16),\r\n                    SampleSubTitle(\"Responsive and Collapsed\"),\r\n                    TextBlock(\"Breadcrumbs will collapse when the container width is restricted.\"),\r\n                    Breadcrumb().MaxWidth(250.px()).Items(\r\n                        Crumb(\"Root\"),\r\n                        Crumb(\"Level 1\"),\r\n                        Crumb(\"Level 2\"),\r\n                        Crumb(\"Level 3\"),\r\n                        Crumb(\"Final\")\r\n                    ).PB(16),\r\n                    SampleSubTitle(\"Small Size and Custom Chevron\"),\r\n                    Breadcrumb().Small().SetChevron(UIcons.AngleRight).Items(\r\n                        Crumb(\"Resources\"),\r\n                        Crumb(\"Icons\"),\r\n                        Crumb(\"UIcons\")\r\n                    ).PB(16),\r\n                    SampleSubTitle(\"Overflow Configuration\"),\r\n                    TextBlock(\"You can control where the overflow starts (e.g., after the second item).\"),\r\n                    Breadcrumb().SetOverflowIndex(1).MaxWidth(200.px()).Items(\r\n                        Crumb(\"Home\"),\r\n                        Crumb(\"App\"),\r\n                        Crumb(\"Module\"),\r\n                        Crumb(\"Feature\"),\r\n                        Crumb(\"Detail\")\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "DetailsListSample": 
                            return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Threading.Tasks;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 20, Icon = UIcons.TableRows)]\r\n    public class DetailsListSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public DetailsListSample()\r\n        {\r\n            var query = Router.GetQueryParameters();\r\n            int page  = query.ContainsKey(\"page\") && query.TryGetValue(\"page\", out var queryPageStr) && int.TryParse(queryPageStr, out var queryPage) ? queryPage : 2;\r\n\r\n            _content = SectionStack()\r\n                   .Title(SampleHeader(nameof(DetailsListSample)))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Overview\"),\r\n                        TextBlock(\"DetailsList is a robust way to display an information-rich collection of items. It supports sorting, grouping, filtering, and pagination.\"),\r\n                        TextBlock(\"It is classically used for file explorers, database record views, or any scenario where information density is critical.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Best Practices\"),\r\n                        TextBlock(\"Use DetailsList when users need to compare items across multiple metadata fields. Display columns in order of importance from left to right. Provide ample default width for each column to avoid unnecessary truncation. Use compact mode when vertical space is limited or when displaying very large datasets. Always provide a clear empty state message if the list contains no items.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        SampleSubTitle(\"Standard File List\"),\r\n                        TextBlock(\"A list with textual rows, supporting sorting and custom column widths.\"),\r\n                        DetailsList<DetailsListSampleFileItem>(\r\n                                IconColumn(Icon(UIcons.File), width: 32.px()),\r\n                                DetailsListColumn(title: \"File Name\", width: 350.px(), enableColumnSorting: true, sortingKey: \"FileName\", isRowHeader: true),\r\n                                DetailsListColumn(title: \"Date Modified\", width: 170.px(), enableColumnSorting: true, sortingKey: \"DateModified\"),\r\n                                DetailsListColumn(title: \"Modified By\", width: 150.px(), enableColumnSorting: true, sortingKey: \"ModifiedBy\"),\r\n                                DetailsListColumn(title: \"File Size\", width: 120.px(), enableColumnSorting: true, sortingKey: \"FileSize\"))\r\n                           .Height(400.px())\r\n                           .WithListItems(GetDetailsListItems())\r\n                           .SortedBy(\"FileName\")\r\n                           .MB(32),\r\n                        SampleSubTitle(\"Responsive Widths\"),\r\n                        TextBlock(\"Using percentage-based widths with max-width constraints.\"),\r\n                        DetailsList<DetailsListSampleFileItem>(\r\n                                IconColumn(Icon(UIcons.File), width: 64.px()),\r\n                                DetailsListColumn(title: \"File Name\", width: 40.percent(), enableColumnSorting: true, sortingKey: \"FileName\", isRowHeader: true),\r\n                                DetailsListColumn(title: \"Date Modified\", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"DateModified\"),\r\n                                DetailsListColumn(title: \"Modified By\", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"ModifiedBy\"),\r\n                                DetailsListColumn(title: \"File Size\", width: 10.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"FileSize\"))\r\n                           .Height(400.px())\r\n                           .WidthStretch()\r\n                           .WithListItems(GetDetailsListItems())\r\n                           .MB(32),\r\n                        SampleSubTitle(\"Interactive Components in Rows\"),\r\n                        TextBlock(\"DetailsList can host any Tesserae component within its cells.\"),\r\n                        DetailsList<DetailsListSampleItemWithComponents>(\r\n                                IconColumn(Icon(UIcons.AppleWhole), width: 32.px()),\r\n                                DetailsListColumn(title: \"Status\", width: 120.px()),\r\n                                DetailsListColumn(title: \"Name\", width: 250.px(), isRowHeader: true),\r\n                                DetailsListColumn(title: \"Action\", width: 150.px()),\r\n                                DetailsListColumn(title: \"Rating\", width: 400.px()))\r\n                           .Compact()\r\n                           .Height(400.px())\r\n                           .WithListItems(GetComponentDetailsListItems())\r\n                           .MB(32),\r\n                        SampleSubTitle(\"Paginated and Empty States\"),\r\n                        SplitView().Resizable().SplitInMiddle().Left(\r\n                            VStack().WS().Children(\r\n                                TextBlock(\"Infinite scrolling\").SemiBold(),\r\n                                DetailsList<DetailsListSampleFileItem>(\r\n                                        IconColumn(Icon(UIcons.File), width: 64.px()),\r\n                                        DetailsListColumn(title: \"File Name\", width: 40.percent(), enableColumnSorting: true, sortingKey: \"FileName\", isRowHeader: true),\r\n                                        DetailsListColumn(title: \"Date Modified\", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"DateModified\"),\r\n                                        DetailsListColumn(title: \"Modified By\", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"ModifiedBy\"),\r\n                                        DetailsListColumn(title: \"File Size\", width: 10.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"FileSize\"))\r\n                                   .Height(300.px())\r\n                                   .WithListItems(GetDetailsListItems(0, page))\r\n                                   .WithPaginatedItems(async () =>\r\n                                    {\r\n                                        page++;\r\n                                        Router.ReplaceQueryParameters(p => p.With(\"page\", page.ToString()));\r\n                                        return await GetDetailsListItemsAsync(page, 5);\r\n                                    })\r\n                            )).Right(\r\n                            VStack().WS().Children(\r\n                                TextBlock(\"Empty State\").SemiBold(),\r\n                                DetailsList<DetailsListSampleFileItem>(\r\n                                        IconColumn(Icon(UIcons.File), width: 64.px()),\r\n                                        DetailsListColumn(title: \"File Name\", width: 40.percent(), enableColumnSorting: true, sortingKey: \"FileName\", isRowHeader: true),\r\n                                        DetailsListColumn(title: \"Date Modified\", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"DateModified\"),\r\n                                        DetailsListColumn(title: \"Modified By\", width: 30.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"ModifiedBy\"),\r\n                                        DetailsListColumn(title: \"File Size\", width: 10.percent(), maxWidth: 150.px(), enableColumnSorting: true, sortingKey: \"FileSize\"))\r\n                                   .WithEmptyMessage(() => BackgroundArea(Card(TextBlock(\"No items found\").Padding(16.px()))).WS().HS())\r\n                                   .Height(300.px())\r\n                                   .WithListItems(new DetailsListSampleFileItem[0])\r\n                            )\r\n                        )\r\n                    ));\r\n        }\r\n\r\n        private async Task<DetailsListSampleFileItem[]> GetDetailsListItemsAsync(int start = 1, int count = 100)\r\n        {\r\n            await Task.Delay(1000);\r\n            return GetDetailsListItems(start, count);\r\n        }\r\n\r\n        private DetailsListSampleFileItem[] GetDetailsListItems(int start = 1, int count = 100)\r\n        {\r\n            return Enumerable.Range(start, count).Select(n => new DetailsListSampleFileItem(UIcons.FileWord, $\"Document_{n}.docx\", DateTime.Today.AddDays(-n), \"System\", n * 1.5)).ToArray();\r\n        }\r\n\r\n        private DetailsListSampleItemWithComponents[] GetComponentDetailsListItems()\r\n        {\r\n            return Enumerable.Range(1, 20).Select(n => new DetailsListSampleItemWithComponents()\r\n                       .WithIcon(UIcons.Database)\r\n                       .WithCheckBox(CheckBox(\"Active\").Checked())\r\n                       .WithName($\"Record {n}\")\r\n                       .WithButton(Button(\"Edit\").Primary().OnClick((s, e) => Toast().Information($\"Editing {n}\")))\r\n                       .WithChoiceGroup(ChoiceGroup().Horizontal().Choices(Choice(\"A\"), Choice(\"B\"), Choice(\"C\")))\r\n                ).ToArray();\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n\r\n    public class DetailsListSampleFileItem : IDetailsListItem<DetailsListSampleFileItem>\r\n    {\r\n        public DetailsListSampleFileItem(UIcons icon, string name, DateTime modified, string by, double size) { FileIcon = icon; FileName = name; DateModified = modified; ModifiedBy = by; FileSize = size; }\r\n        public UIcons FileIcon { get; }\r\n        public string FileName { get; }\r\n        public DateTime DateModified { get; }\r\n        public string ModifiedBy { get; }\r\n        public double FileSize { get; }\r\n        public bool EnableOnListItemClickEvent => true;\r\n        public void OnListItemClick(int index) => Toast().Information($\"Clicked: {FileName}\");\r\n        public int CompareTo(DetailsListSampleFileItem other, string key)\r\n        {\r\n            switch (key)\r\n            {\r\n                case \"FileName\":     return string.Compare(FileName, other.FileName, StringComparison.OrdinalIgnoreCase);\r\n                case \"DateModified\": return DateModified.CompareTo(other.DateModified);\r\n                case \"ModifiedBy\":   return string.Compare(ModifiedBy, other.ModifiedBy, StringComparison.OrdinalIgnoreCase);\r\n                case \"FileSize\":     return FileSize.CompareTo(other.FileSize);\r\n                default:             return 0;\r\n            }\r\n        }\r\n        public IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> cell)\r\n        {\r\n            yield return cell(columns[0], () => Icon(FileIcon));\r\n            yield return cell(columns[1], () => TextBlock(FileName));\r\n            yield return cell(columns[2], () => TextBlock(DateModified.ToShortDateString()));\r\n            yield return cell(columns[3], () => TextBlock(ModifiedBy));\r\n            yield return cell(columns[4], () => TextBlock(FileSize.ToString(\"N1\") + \" KB\"));\r\n        }\r\n    }\r\n\r\n    public class DetailsListSampleItemWithComponents : IDetailsListItem<DetailsListSampleItemWithComponents>\r\n    {\r\n        public UIcons Icon { get; private set; }\r\n        public CheckBox CheckBox { get; private set; }\r\n        public string Name { get; private set; }\r\n        public Button Button { get; private set; }\r\n        public ChoiceGroup ChoiceGroup { get; private set; }\r\n        public bool EnableOnListItemClickEvent => false;\r\n        public void OnListItemClick(int index) {}\r\n        public int CompareTo(DetailsListSampleItemWithComponents other, string key) => 0;\r\n        public DetailsListSampleItemWithComponents WithIcon(UIcons icon) { Icon = icon; return this; }\r\n        public DetailsListSampleItemWithComponents WithCheckBox(CheckBox cb) { CheckBox = cb; return this; }\r\n        public DetailsListSampleItemWithComponents WithName(string name) { Name = name; return this; }\r\n        public DetailsListSampleItemWithComponents WithButton(Button btn) { Button = btn; return this; }\r\n        public DetailsListSampleItemWithComponents WithChoiceGroup(ChoiceGroup cg) { ChoiceGroup = cg; return this; }\r\n        public IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> cell)\r\n        {\r\n            yield return cell(columns[0], () => Icon(Icon));\r\n            yield return cell(columns[1], () => CheckBox);\r\n            yield return cell(columns[2], () => TextBlock(Name));\r\n            yield return cell(columns[3], () => Button);\r\n            yield return cell(columns[4], () => ChoiceGroup);\r\n        }\r\n    }\r\n}\r\n";
                        case "InfiniteScrollingListSample": 
                            return "using System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Threading.Tasks;\r\nusing Tesserae;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 20, Icon = UIcons.Infinity)]\r\n    public class InfiniteScrollingListSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public InfiniteScrollingListSample()\r\n        {\r\n            var page     = 1;\r\n            var pageGrid = 1;\r\n\r\n            _content = SectionStack().WidthStretch()\r\n               .Title(SampleHeader(nameof(InfiniteScrollingListSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"InfiniteScrollingList provides a way to render large sets of items by loading them on demand as the user scrolls. It uses a visibility sensor to detect when the end of the list is reached.\"),\r\n                    TextBlock(\"This approach is great for social feeds, search results, or any collection where you want to avoid explicit pagination buttons.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use infinite scrolling for content that is explored discovery-style rather than searched for specifically. Ensure that the loading state is clearly indicated to the user. Consider the performance impact of adding many DOM elements; for extremely large lists, VirtualizedList may be more appropriate. Provide a way for users to reach the footer of the page if necessary, perhaps by offering a 'Load More' button instead of fully automatic scrolling if the footer contains important links.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Vertical Infinite List\"),\r\n                    TextBlock(\"Items are loaded 20 at a time with a small delay to simulate network latency.\"),\r\n                    InfiniteScrollingList(GetSomeItems(20, 0, \" (Initial Set)\"), async () => await GetSomeItemsAsync(20, page++)).Height(400.px()).MB(32),\r\n                    SampleSubTitle(\"Grid-based Infinite List\"),\r\n                    TextBlock(\"Displaying items in a 3-column grid that expands as you scroll.\"),\r\n                    InfiniteScrollingList(GetSomeItems(20, 0, \" (Initial Set)\"), async () => await GetSomeItemsAsync(20, pageGrid++), 33.percent(), 33.percent(), 34.percent()).Height(400.px())\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n\r\n        private IComponent[] GetSomeItems(int count, int page = -1, string txt = \"\")\r\n        {\r\n            var pageString = page > 0 ? $\"Page {page}\" : \"\";\r\n            return Enumerable.Range(1, count).Select(n => Card(TextBlock($\"{pageString} - Item {n}{txt}\").NonSelectable()).MinWidth(200.px())).ToArray();\r\n        }\r\n\r\n        private async Task<IComponent[]> GetSomeItemsAsync(int count, int page = -1, string txt = \"\")\r\n        {\r\n            await Task.Delay(500);\r\n            return GetSomeItems(count, page, txt);\r\n        }\r\n    }\r\n}\r\n";
                        case "ItemsListSample": 
                            return "using System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 20, Icon = UIcons.List)]\r\n    public class ItemsListSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ItemsListSample()\r\n        {\r\n            var obsList = new ObservableList<IComponent>();\r\n            var vs = VisibilitySensor((v) =>\r\n            {\r\n                obsList.Remove(v);\r\n                obsList.AddRange(GetSomeItems(10, \" (Dynamic)\"));\r\n                v.Reset();\r\n                obsList.Add(v);\r\n            });\r\n            obsList.AddRange(GetSomeItems(10, \" (Initial)\"));\r\n            obsList.Add(vs);\r\n\r\n            _content = SectionStack().WidthStretch()\r\n               .Title(SampleHeader(nameof(ItemsListSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ItemsList is a versatile component for displaying collections of items. It supports static lists, observable lists, and grid layouts.\"),\r\n                    TextBlock(\"It is ideal for smaller to medium-sized collections. For very large datasets, consider using VirtualizedList for better performance.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use ItemsList when you want full control over the rendering of each item. Leverage observable lists to automatically update the UI when the underlying data changes. Use columns to create grid layouts that adapt to the container width. Always provide a meaningful empty message if there are no items to display. If you expect a very high number of items, ensure you test the performance or switch to a virtualized approach.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Simple List\"),\r\n                    ItemsList(GetSomeItems(5)).Height(250.px()).MB(32),\r\n                    SampleSubTitle(\"Multi-column Grid\"),\r\n                    ItemsList(GetSomeItems(12), 33.percent(), 33.percent(), 34.percent()).Height(300.px()).MB(32),\r\n                    SampleSubTitle(\"Dynamic Observable List\"),\r\n                    TextBlock(\"This list uses a VisibilitySensor to append more items as you scroll.\"),\r\n                    ItemsList(obsList, 50.percent(), 50.percent()).Height(300.px()).MB(32),\r\n                    SampleSubTitle(\"Empty State\"),\r\n                    ItemsList(new IComponent[0])\r\n                       .WithEmptyMessage(() => BackgroundArea(Card(TextBlock(\"No items to show\").Padding(16.px()))).WS().HS().MinHeight(100.px()))\r\n                       .Height(150.px())\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n\r\n        private IComponent[] GetSomeItems(int count, string suffix = \"\")\r\n        {\r\n            return Enumerable.Range(1, count).Select(n => Card(TextBlock($\"Item {n}{suffix}\")).MinWidth(150.px())).ToArray();\r\n        }\r\n    }\r\n}\r\n";
                        case "MasonrySample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 0, Icon = UIcons.Grid)]\r\n    public class MasonrySample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public MasonrySample()\r\n        {\r\n            _content = SectionStack().S()\r\n               .Title(SampleHeader(nameof(MasonrySample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Masonry layout (also known as a Pinterest-style layout) is a grid where items are placed in optimal positions based on available vertical space.\"),\r\n                    TextBlock(\"Unlike a standard grid where rows have a uniform height, a Masonry layout allows items of varying heights to be packed tightly together.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Masonry for visually-driven content like image galleries or dashboard widgets with varying heights. Ensure that the number of columns is appropriate for the screen size. Provide a consistent gap between items to maintain a clean appearance. Avoid using Masonry for content that needs to be read in a specific sequential order, as the placement can be non-linear.\")))\r\n               .Section(VStack().S().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Masonry Grid (4 Columns)\"),\r\n                    Masonry(4).S().Children(GetCards(50).ToArray()).ScrollY()\r\n                ), grow: true);\r\n        }\r\n\r\n        private IEnumerable<IComponent> GetCards(int count)\r\n        {\r\n            var rng = new Random();\r\n            for (int i = 0; i < count; i++)\r\n            {\r\n                var height = 80 + (int)(rng.NextDouble() * 4) * 40;\r\n                yield return Card(VStack().AlignCenter().JustifyContent(ItemJustify.Center).Children(TextBlock($\"Card {i}\"))).H(height.px()).W(100.percent());\r\n            }\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "ObservableStackSample": 
                            return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing H5;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 0, Icon = UIcons.RulerVertical)]\r\n    public class ObservableStackSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        private static int _elementIndex = 4;\r\n        private static ObservableList<IComponentWithID> _stackElementsList;\r\n\r\n        public class StackElement : IComponentWithID\r\n        {\r\n            public string Id { get; set; }\r\n            public string DisplayName { get; set; }\r\n            public string Color { get; }\r\n            public string Identifier => Id;\r\n            public string ContentHash => HashingHelper.Fnv1aHash(Id + DisplayName).ToString();\r\n\r\n            public StackElement(string id, string displayName)\r\n            {\r\n                Id = id;\r\n                DisplayName = displayName;\r\n                Color = '#' + Math.Floor(Math.Random() * 16777215).ToString(16).PadLeft(6, '0');\r\n            }\r\n\r\n            public HTMLElement Render()\r\n            {\r\n                return Card(VStack().Children(\r\n                    Label(\"Name\").Inline().AutoWidth().SetContent(TextBlock(DisplayName)),\r\n                    Label(\"ID\").Inline().AutoWidth().SetContent(TextBlock(Id)),\r\n                    Label(\"Hash\").Inline().AutoWidth().SetContent(TextBlock(ContentHash).Background(Color).Padding(4.px()))\r\n                ).Class(\"flash-bg\")).Render();\r\n            }\r\n        }\r\n\r\n        public ObservableStackSample()\r\n        {\r\n            _stackElementsList = new ObservableList<IComponentWithID>();\r\n            _stackElementsList.ReplaceAll(Enumerable.Range(0, 4).Select(i => new StackElement(i.ToString(), $\"Item {i}\")).ToArray());\r\n\r\n            var obsStack = new ObservableStack(_stackElementsList, debounce: true);\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ObservableStackSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ObservableStack is a specialized container that synchronizes its DOM with an observable list using an efficient reconciliation process.\"),\r\n                    TextBlock(\"Instead of re-rendering the entire list when a change occurs, it identifies which elements were added, removed, or moved by comparing their unique Identifiers and ContentHashes. This makes it ideal for high-performance lists where preserving scroll position or component state is important.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use ObservableStack when your list data changes frequently or when you want smooth transitions for moved items. Ensure each item implements 'IComponentWithID' correctly, providing a stable 'Identifier' and a 'ContentHash' that reflects any changes in the item's data. Avoid frequent full-list replacements if only a few items have changed. Leverage the reconciliation behavior to keep the DOM footprint minimal and performance high.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Interactive Reconciliation Demo\"),\r\n                    TextBlock(\"Modify the list below and watch how the 'Display Elements' on the right update efficiently.\"),\r\n                    SplitView().Height(500.px()).WS()\r\n                       .Left(VStack().Children(\r\n                            HStack().Children(\r\n                                Button(\"Randomize Order\").OnClick(() => _stackElementsList.ReplaceAll(_stackElementsList.Value.OrderBy(_ => Math.Random()).ToList())),\r\n                                Button(\"Add New Item\").Primary().OnClick(() => AddItem())\r\n                            ).MB(16),\r\n                            Label(\"Edit items in-place:\").SemiBold(),\r\n                            DeferSync(_stackElementsList, elements =>\r\n                            {\r\n                                var list = VStack();\r\n                                for (int i = 0; i < elements.Count; i++)\r\n                                {\r\n                                    var idx = i;\r\n                                    var item = elements[i] as StackElement;\r\n                                    list.Add(HStack().AlignItemsCenter().Children(\r\n                                        Button().SetIcon(UIcons.ArrowUp).OnClick(() => Move(idx, idx - 1)),\r\n                                        Button().SetIcon(UIcons.ArrowDown).OnClick(() => Move(idx, idx + 1)),\r\n                                        TextBox(item.DisplayName).OnBlur((tb, _) => { item.DisplayName = tb.Text; Update(idx, item); }).Background(item.Color).WS()\r\n                                    ).MB(4));\r\n                                }\r\n                                return list.ScrollY();\r\n                            }).Grow()\r\n                       ).P(8))\r\n                       .Right(VStack().Children(\r\n                            Label(\"Rendered Stack:\").SemiBold(),\r\n                            obsStack.H(450.px()).WS()\r\n                       ).P(8))\r\n                ));\r\n        }\r\n\r\n        private void Move(int oldIdx, int newIdx)\r\n        {\r\n            if (newIdx < 0 || newIdx >= _stackElementsList.Count) return;\r\n            var list = _stackElementsList.Value.ToList();\r\n            var item = list[oldIdx];\r\n            list.RemoveAt(oldIdx);\r\n            list.Insert(newIdx, item);\r\n            _stackElementsList.ReplaceAll(list);\r\n        }\r\n\r\n        private void Update(int idx, StackElement item)\r\n        {\r\n            var list = _stackElementsList.Value.ToList();\r\n            list[idx] = item;\r\n            _stackElementsList.ReplaceAll(list);\r\n        }\r\n\r\n        private void AddItem()\r\n        {\r\n            _elementIndex++;\r\n            var list = _stackElementsList.Value.ToList();\r\n            list.Add(new StackElement(_elementIndex.ToString(), $\"Item {_elementIndex}\"));\r\n            _stackElementsList.ReplaceAll(list);\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n\r\n    public static class HashingHelper\r\n    {\r\n        public static int Fnv1aHash(string value)\r\n        {\r\n            var valArray = value.ToCharArray();\r\n            var hash = 0x811c9dc5;\r\n            for (var i = 0; i < valArray.Length; i++) { hash ^= (uint)valArray[i]; hash *= 0x01000193; }\r\n            return (int)hash;\r\n        }\r\n    }\r\n}\r\n";
                        case "OverflowSetSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 10, Icon = UIcons.MenuDots)]\r\n    public class OverflowSetSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public OverflowSetSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(OverflowSetSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"OverflowSet is a container that automatically moves items that don't fit into the available space into an overflow menu.\"),\r\n                    TextBlock(\"It is commonly used for command bars, navigation menus, or any list of actions where you want to maximize the visibility of primary items while ensuring all items are accessible.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use OverflowSet when you have a horizontal list of items that might exceed the screen width. Order items by importance so that the most critical actions are the last to be moved to the overflow menu. Provide a clear icon or label for the overflow trigger (usually a 'more' icon). Ensure that items in the overflow menu remain fully functional.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic OverflowSet\"),\r\n                    TextBlock(\"Resize the window or container to see items moving into the '...' menu.\"),\r\n                    OverflowSet().Items(\r\n                        Button(\"Action 1\").Link().OnClick((s, e) => Toast().Information(\"Action 1\")),\r\n                        Button(\"Action 2\").Link().OnClick((s, e) => Toast().Information(\"Action 2\")),\r\n                        Button(\"Action 3\").Link().OnClick((s, e) => Toast().Information(\"Action 3\")),\r\n                        Button(\"Action 4\").Link().OnClick((s, e) => Toast().Information(\"Action 4\")),\r\n                        Button(\"Action 5\").Link().OnClick((s, e) => Toast().Information(\"Action 5\")),\r\n                        Button(\"Action 6\").Link().OnClick((s, e) => Toast().Information(\"Action 6\"))\r\n                    ).PB(32),\r\n                    SampleSubTitle(\"With Icons and Constraints\"),\r\n                    OverflowSet().MaxWidth(300.px()).Items(\r\n                        Button(\"Edit\").SetIcon(UIcons.Edit).Link(),\r\n                        Button(\"Share\").SetIcon(UIcons.Share).Link(),\r\n                        Button(\"Delete\").SetIcon(UIcons.Trash).Link(),\r\n                        Button(\"Copy\").SetIcon(UIcons.Copy).Link(),\r\n                        Button(\"Move\").SetIcon(UIcons.Arrows).Link()\r\n                    ).PB(32),\r\n                    SampleSubTitle(\"Custom Overflow Index\"),\r\n                    TextBlock(\"Force overflow to start after the first item:\"),\r\n                    OverflowSet().SetOverflowIndex(0).MaxWidth(400.px()).Items(\r\n                        Button(\"Always Visible\").Primary(),\r\n                        Button(\"Option A\").Link(),\r\n                        Button(\"Option B\").Link(),\r\n                        Button(\"Option C\").Link()\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SearchableGroupedListSample": 
                            return "using System;\r\nusing System.Linq;\r\nusing Tesserae;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 20, Icon = UIcons.Search)]\r\n    public class SearchableGroupedListSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SearchableGroupedListSample()\r\n        {\r\n            _content = SectionStack().WidthStretch()\r\n                   .Title(SampleHeader(nameof(SearchableGroupedListSample)))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Overview\"),\r\n                        TextBlock(\"SearchableGroupedList extends the functionality of SearchableList by adding automatic grouping of items based on a 'Group' property.\"),\r\n                        TextBlock(\"It provides a structured way to display filtered results, categorized by logical groups like file types, departments, or priority levels.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use SearchableGroupedList when your dataset has a natural hierarchy or categorization that helps users find items faster. Provide a clear header for each group using the header generator. Ensure that the 'IsMatch' logic considers both the item content and the group name if appropriate. Like SearchableList, provide a meaningful 'No Results' message and use additional command slots for relevant actions.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Grouped Search with Custom Headers\"),\r\n                    SearchableGroupedList(GetItems(20), s => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left())\r\n                       .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock(\"No matching records\").Padding(16.px()))).WS().HS().MinHeight(100.px()))\r\n                       .Height(400.px()).MB(32),\r\n                    SampleSubTitle(\"Grouped Grid Layout\"),\r\n                    SearchableGroupedList(GetItems(40), s => Label(s).Primary().Bold(), 33.percent(), 33.percent(), 34.percent())\r\n                       .Height(500.px())\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n\r\n        private SearchableGroupedListItem[] GetItems(int count)\r\n        {\r\n            return Enumerable.Range(1, count).Select((n, i) =>\r\n                new SearchableGroupedListItem($\"Record {n}\", (i % 3 == 0) ? \"Category A\" : (i % 2 == 0) ? \"Category B\" : \"Category C\")\r\n            ).ToArray();\r\n        }\r\n\r\n        private class SearchableGroupedListItem : ISearchableGroupedItem\r\n        {\r\n            private readonly string _value;\r\n            private readonly IComponent _component;\r\n            public SearchableGroupedListItem(string value, string group) { _value = value; Group = group; _component = Card(TextBlock(value)); }\r\n            public bool IsMatch(string searchTerm) => _value.ToLower().Contains(searchTerm.ToLower()) || Group.ToLower().Contains(searchTerm.ToLower());\r\n            public string Group { get; }\r\n            public IComponent Render() => _component;\r\n        }\r\n    }\r\n}\r\n";
                        case "SearchableListSample": 
                            return "using System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 20, Icon = UIcons.Search)]\r\n    public class SearchableListSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SearchableListSample()\r\n        {\r\n            _content = SectionStack().WidthStretch()\r\n                   .Title(SampleHeader(nameof(SearchableListSample)))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Overview\"),\r\n                        TextBlock(\"SearchableList combines a search box with a list of items, providing instant filtering as the user types.\"),\r\n                        TextBlock(\"Items must implement the 'ISearchableItem' interface, which defines the matching logic and how each item is rendered.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Best Practices\"),\r\n                        TextBlock(\"Use SearchableList when you have a moderately sized collection that users need to filter quickly. Ensure the 'IsMatch' implementation is performant and covers all relevant fields. Provide a clear 'No Results' message to help users understand when their search doesn't match anything. Use the 'BeforeSearchBox' and 'AfterSearchBox' slots to add relevant actions like 'Add New' or 'Filter' buttons. For very large datasets, consider server-side filtering or a VirtualizedList.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        SampleSubTitle(\"Basic Searchable List\"),\r\n                        SearchableList(GetItems(10))\r\n                           .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock(\"No matching items found\").Padding(16.px()))).WS().HS().MinHeight(100.px()))\r\n                           .Height(400.px()).MB(32),\r\n                        SampleSubTitle(\"Searchable Grid with Commands\"),\r\n                        SearchableList(GetItems(24), 25.percent(), 25.percent(), 25.percent(), 25.percent())\r\n                           .BeforeSearchBox(Button(\"Filter\").SetIcon(UIcons.Filter))\r\n                           .AfterSearchBox(Button(\"Add Item\").Primary().SetIcon(UIcons.Plus))\r\n                           .Height(400.px())\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n\r\n        private SearchableListItem[] GetItems(int count)\r\n        {\r\n            return Enumerable.Range(1, count).Select(n => new SearchableListItem($\"Item {n}\")).ToArray();\r\n        }\r\n\r\n        private class SearchableListItem : ISearchableItem\r\n        {\r\n            private readonly string _value;\r\n            private readonly IComponent _component;\r\n            public SearchableListItem(string value) { _value = value; _component = Card(TextBlock(value)); }\r\n            public bool IsMatch(string searchTerm) => _value.ToLower().Contains(searchTerm.ToLower());\r\n            public HTMLElement Render() => _component.Render();\r\n            IComponent ISearchableItem.Render() => _component;\r\n        }\r\n    }\r\n}\r\n";
                        case "StackSample": 
                            return "using System;\r\nusing H5.Core;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 0, Icon = UIcons.RulerVertical)]\r\n    public class StackSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        private const string sampleSortableStackLocalStorageKey = \"sampleSortableStackOrder\";\r\n\r\n        public StackSample()\r\n        {\r\n            var mainButton = Button(\"Hover me\").TextLeft().MinWidth(200.px());\r\n            var otherButton = Button().SetIcon(UIcons.ThumbsDown, color: Theme.Danger.Background).Fade();\r\n            var hoverStack = HStack().MaxWidth(500.px()).Children(mainButton, otherButton);\r\n\r\n            var sortableStack = new SortableStack(Stack.Orientation.Horizontal).WS().AlignItemsCenter().PB(8).MaxWidth(500.px());\r\n            sortableStack.Add(\"1\", Button().SetIcon(UIcons._1));\r\n            sortableStack.Add(\"2\", Button().SetIcon(UIcons._2));\r\n            sortableStack.Add(\"3\", Button().SetIcon(UIcons._3));\r\n            sortableStack.Add(\"4\", Button().SetIcon(UIcons._4));\r\n            sortableStack.Add(\"5\", Button().SetIcon(UIcons._5));\r\n\r\n            var stack = Stack();\r\n            var countSlider = Slider(5, 0, 10, 1);\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(StackSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Stacks are container components that simplify the use of Flexbox for layout. They allow you to arrange children components either horizontally (HStack) or vertically (VStack).\"),\r\n                    TextBlock(\"Tesserae's Stack also includes advanced features like 'SortableStack' for drag-and-drop reordering.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Stacks as the primary way to organize your UI elements. Use HStack for side-by-side components and VStack for top-to-bottom arrangements. Leverage the 'Gap' property to ensure consistent spacing between children. Use SortableStack when users need to customize the order of items, such as in a dashboard or task list. Avoid deeply nested stacks if a Grid layout would be more appropriate for the complexity.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Live Layout Playground\"),\r\n                    VStack().Children(\r\n                        HStack().Children(\r\n                            VStack().Children(\r\n                                Label(\"Number of items:\").SetContent(countSlider.OnInput((s, e) => SetChildren(stack, s.Value))),\r\n                                ChoiceGroup(\"Orientation:\").Horizontal().Choices(Choice(\"Vertical\").Selected(), Choice(\"Horizontal\"), Choice(\"Vertical Reverse\"), Choice(\"Horizontal Reverse\")).OnChange((s, e) => {\r\n                                    if (s.SelectedOption.Text == \"Horizontal\") stack.Horizontal();\r\n                                    else if (s.SelectedOption.Text == \"Vertical\") stack.Vertical();\r\n                                    else if (s.SelectedOption.Text == \"Horizontal Reverse\") stack.HorizontalReverse();\r\n                                    else if (s.SelectedOption.Text == \"Vertical Reverse\") stack.VerticalReverse();\r\n                                })\r\n                            )\r\n                        ).MB(16),\r\n                        Card(stack.HeightAuto())\r\n                    ),\r\n                    SampleSubTitle(\"Sortable Stack\"),\r\n                    TextBlock(\"Drag and drop these buttons to reorder them. The state is saved to local storage.\"),\r\n                    sortableStack.MB(32),\r\n                    SampleSubTitle(\"Interactive Events\"),\r\n                    TextBlock(\"Stacks can respond to mouse events, allowing for complex hover behaviors.\"),\r\n                    hoverStack.OnMouseOver((s, e) => otherButton.Show()).OnMouseOut((s, e) => otherButton.Fade()),\r\n                    SampleSubTitle(\"Rounded Stacks\"),\r\n                    TextBlock(\"Rounding is visible when the stack has a background or border.\"),\r\n                    HStack().Children(\r\n                        VStack().Children(TextBlock(\"Small\")).P(16).Rounded(BorderRadius.Small).Background(Theme.Colors.Blue200).W(100).AlignItemsCenter(),\r\n                        VStack().Children(TextBlock(\"Medium\")).P(16).Rounded(BorderRadius.Medium).Background(Theme.Colors.Blue200).W(100).AlignItemsCenter(),\r\n                        VStack().Children(TextBlock(\"Full\")).P(16).Rounded(BorderRadius.Full).Background(Theme.Colors.Blue200).W(100).AlignItemsCenter()\r\n                    )\r\n                ));\r\n            SetChildren(stack, 5);\r\n        }\r\n\r\n        private void SetChildren(Stack stack, int count) { stack.Clear(); for (int i = 0; i < count; i++) stack.Add(Button($\"Item {i}\")); }\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "TimelineSample": 
                            return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 20, Icon = UIcons.TimePast)]\r\n    public class TimelineSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public TimelineSample()\r\n        {\r\n            _content = SectionStack().WidthStretch()\r\n               .Title(SampleHeader(nameof(TimelineSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Timeline displays a series of events in chronological order, using a vertical line to connect them.\"),\r\n                    TextBlock(\"It is ideal for activity feeds, version histories, or any process where the sequence of steps is important.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Timelines to show the progression of time or a sequence of related events. Clearly distinguish between past, current, and future events if applicable. Use the 'SameSide' property if you want a more linear, left-aligned layout, or the default staggered layout for a more balanced visual look. Ensure that each event has a clear timestamp and a concise description.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Default Staggered Timeline\"),\r\n                    Timeline().Children(GetSomeItems(6)).Height(300.px()).MB(32),\r\n                    SampleSubTitle(\"Same Side Alignment\"),\r\n                    Timeline().SameSide().Children(GetSomeItems(6)).Height(300.px()).MB(32),\r\n                    SampleSubTitle(\"Constrained Width\"),\r\n                    Timeline().TimelineWidth(400.px()).Children(GetSomeItems(6)).Height(300.px())\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n\r\n        private IComponent[] GetSomeItems(int count)\r\n        {\r\n            return Enumerable.Range(1, count).Select(n =>\r\n                VStack().Children(\r\n                    TextBlock($\"Event {n}\").SemiBold(),\r\n                    TextBlock($\"{DateTime.Today.AddHours(-n):t} - Description of the event happens here.\").Small()\r\n                )).ToArray();\r\n        }\r\n    }\r\n}\r\n";
                        case "VirtualizedListSample": 
                            return "using System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Collections\", Order = 20, Icon = UIcons.List)]\r\n    public class VirtualizedListSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public VirtualizedListSample()\r\n        {\r\n            _content = SectionStack()\r\n                   .Title(SampleHeader(nameof(VirtualizedListSample)))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Overview\"),\r\n                        TextBlock(\"VirtualizedList is a high-performance component designed for rendering massive datasets\u2014thousands or even tens of thousands of items\u2014without sacrificing UI responsiveness.\"),\r\n                        TextBlock(\"It achieves this by only rendering the items that are currently visible within the viewport (plus a small buffer), significantly reducing the number of DOM elements the browser needs to manage.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Best Practices\"),\r\n                        TextBlock(\"Use VirtualizedList for any list that could potentially contain more than a few hundred items. Ensure that each item has a consistent height for accurate scroll position calculation. Virtualization is most effective when item components are relatively complex or resource-intensive to render. Always provide a clear 'Empty Message' if the dataset is expected to be empty.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        SampleSubTitle(\"Virtualized List with 5,000 Items\"),\r\n                        TextBlock(\"Scroll rapidly to see how the list handles a large number of items.\"),\r\n                        VirtualizedList().WithListItems(GetALotOfItems()).Height(400.px()).MB(32),\r\n                        SampleSubTitle(\"Empty State\"),\r\n                        VirtualizedList()\r\n                           .WithEmptyMessage(() => BackgroundArea(Card(TextBlock(\"No items available\"))).WS().HS().MinHeight(100.px()))\r\n                           .WithListItems(Enumerable.Empty<IComponent>())\r\n                           .Height(150.px())\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n\r\n        private IEnumerable<SampleVirtualizedItem> GetALotOfItems()\r\n        {\r\n            return Enumerable.Range(1, 5000).Select(n => new SampleVirtualizedItem($\"Virtualized Item {n}\"));\r\n        }\r\n\r\n        public sealed class SampleVirtualizedItem : IComponent\r\n        {\r\n            private readonly HTMLElement _innerElement;\r\n            public SampleVirtualizedItem(string text) { _innerElement = Div(_(text: text, styles: s => { s.display = \"flex\"; s.alignItems = \"center\"; s.padding = \"0 16px\"; s.height = \"40px\"; s.borderBottom = \"1px solid #eee\"; })); }\r\n            public HTMLElement Render() => _innerElement;\r\n        }\r\n    }\r\n}\r\n";
                        case "AccordionSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.Accordion)]\r\n    public class AccordionSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public AccordionSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(AccordionSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"An accordion contains a list of expanders that can be toggled to reveal more information. They are useful for organizing content into manageable chunks and reducing vertical space usage when not all information needs to be visible at once.\"),\r\n                    TextBlock(\"Tesserae's Accordion component manages multiple Expanders, allowing you to control whether one or multiple sections can be open at the same time.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use accordions to organize related content that might be too long to display all at once. Ensure the header of each expander clearly describes the content within. Avoid nesting accordions within accordions as it can lead to confusion. Consider using a single Expander if you only have one block of optional content.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Accordion\"),\r\n                    Accordion(\r\n                        Expander(\"Getting started\", TextBlock(\"Use expanders to reveal details in place without navigating away.\")).Expanded(),\r\n                        Expander(\"Configuration\", Stack().Children(\r\n                            TextBlock(\"You can nest any component inside an expander.\"),\r\n                            Button(\"Primary action\").Primary())),\r\n                        Expander(\"Advanced\", TextBlock(\"Combine with SectionStack or Card for complex layouts.\")))\r\n                       .AllowMultipleOpen(false),\r\n                    SampleSubTitle(\"Accordion with Multiple Open Allowed\"),\r\n                    Accordion(\r\n                        Expander(\"Section 1\", TextBlock(\"Multiple sections can be open simultaneously here.\")),\r\n                        Expander(\"Section 2\", TextBlock(\"This is useful for comparing information between sections.\")),\r\n                        Expander(\"Section 3\", TextBlock(\"Just set .AllowMultipleOpen(true) on the accordion.\")))\r\n                       .AllowMultipleOpen(true),\r\n                    SampleSubTitle(\"Standalone Expander\"),\r\n                    Expander(\"What is Tesserae?\", TextBlock(\"Tesserae provides a fluent API for building UI components.\"))\r\n                       .Expanded()));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "ActionButtonSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 5, Icon = UIcons.CursorFingerClick)]\r\n    public class ActionButtonSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ActionButtonSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ActionButtonSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ActionButtons are a variation of the standard Button component that split the interaction into two distinct parts: a display area (typically the label and an icon) and a specific action area (typically a secondary icon on the right).\"),\r\n                    TextBlock(\"They are useful when you want to provide a primary action while also offering a secondary, related action like opening a menu, showing a tooltip, or triggering a specific sub-task.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use ActionButtons when a component needs to perform more than one related task. The primary area should trigger the most common action, while the secondary area (the action icon) should trigger a complementary one. Clearly distinguish between the two areas visually if they perform very different tasks. Ensure that both interaction points have appropriate tooltips or labels if their purpose isn't immediately obvious.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Action Buttons\"),\r\n                    VStack().Children(\r\n                        ActionButton(\"Standard Action\").Var(out var btn1)\r\n                           .OnClickDisplay((s, e) => Toast().Information(\"Clicked display!\"))\r\n                           .OnClickAction((s,  e) => Toast().Information(\"Clicked action icon!\")),\r\n                        ActionButton(\"Primary with Calendar Icon\", actionIcon: UIcons.Calendar).Primary()\r\n                           .OnClickDisplay((s, e) => Toast().Success(\"Main area clicked\"))\r\n                           .OnClickAction((s,  e) => Toast().Success(\"Calendar icon clicked\")),\r\n                        ActionButton(\"Danger Action\", displayIcon: UIcons.TriangleWarning).Danger()\r\n                           .OnClickDisplay((s, e) => Toast().Error(\"Danger area clicked\"))\r\n                           .OnClickAction((s,  e) => Toast().Error(\"Warning icon clicked\"))\r\n                    ),\r\n                    SampleSubTitle(\"Complex Content\"),\r\n                    VStack().Children(\r\n                        ActionButton(VStack().Children(\r\n                            HStack().AlignItemsCenter().Children(Icon(UIcons.Arrows), TextBlock(\"Move Item\").SemiBold().PL(8)),\r\n                            TextBlock(\"Use this to reorganize your workspace\").Tiny().PT(4)\r\n                        ))\r\n                        .OnClickDisplay((s, e) => Toast().Information(\"Moving...\"))\r\n                        .OnClickAction((s,  e) => Toast().Information(\"Configure move...\")),\r\n                        ActionButton(\"Action with Custom Tooltip\").ModifyActionButton(btn =>\r\n                        {\r\n                            Raw(btn).Tooltip(\"This tooltip is applied to the entire component\");\r\n                        })\r\n                    ),\r\n                    SampleSubTitle(\"Dropdown Simulation\"),\r\n                    ActionButton(\"Show Options\", actionIcon: UIcons.AngleDown).Primary()\r\n                       .OnClickAction((s, e) =>\r\n                        {\r\n                            Action hideAction = null;\r\n                            var menu = VStack().Children(\r\n                                Button(\"Option 1\").OnClick(() => { Toast().Information(\"Option 1\"); hideAction?.Invoke(); }),\r\n                                Button(\"Option 2\").OnClick(() => { Toast().Information(\"Option 2\"); hideAction?.Invoke(); }),\r\n                                Button(\"Option 3\").OnClick(() => { Toast().Information(\"Option 3\"); hideAction?.Invoke(); })\r\n                            ).Render();\r\n                            Tippy.ShowFor(s, menu, out hideAction, TooltipAnimation.None, TooltipPlacement.BottomEnd, 0, 0, 350, true, null);\r\n                        })\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "AvatarSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.User)]\r\n    public class AvatarSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public AvatarSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(AvatarSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Avatars are used to represent users, teams, or entities in the system. They can display images, initials, and presence indicators.\"),\r\n                    TextBlock(\"The Persona component builds upon Avatar by adding textual information like name, role, and status, making it ideal for profile cards or contact lists.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use avatars to provide visual recognition for users. Always provide initials as a fallback for when images fail to load or aren't available. Use the appropriate size for the context\u2014smaller for lists or chat, larger for profiles. Presence indicators should be used when real-time availability information is relevant to the user's task.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Avatar Sizes and Presence\"),\r\n                    TextBlock(\"Avatars support various sizes from XSmall to XLarge and optional presence states.\"),\r\n                    HStack().Children(\r\n                        Avatar(initials: \"JD\", image: \"https://cataas.com/cat\").Size(AvatarSize.XSmall),\r\n                        Avatar(initials: \"JD\", image: \"https://cataas.com/cat\").Size(AvatarSize.Small).Presence(AvatarPresence.Online),\r\n                        Avatar(initials: \"JD\", image: \"https://cataas.com/cat\").Size(AvatarSize.Medium).Presence(AvatarPresence.Away),\r\n                        Avatar(initials: \"JD\", image: \"https://cataas.com/cat\").Size(AvatarSize.Large).Presence(AvatarPresence.Busy),\r\n                        Avatar(initials: \"JD\", image: \"https://cataas.com/cat\").Size(AvatarSize.XLarge).Presence(AvatarPresence.Offline)),\r\n                    SampleSubTitle(\"Initials Fallback\"),\r\n                    TextBlock(\"When no image is provided, initials are displayed with a generated background color.\"),\r\n                    HStack().Children(\r\n                        Avatar(initials: \"JD\").Size(AvatarSize.Small).Presence(AvatarPresence.Online),\r\n                        Avatar(initials: \"AS\").Size(AvatarSize.Medium).Presence(AvatarPresence.Away),\r\n                        Avatar(initials: \"KL\").Size(AvatarSize.Large).Presence(AvatarPresence.Busy),\r\n                        Avatar(initials: \"MW\").Size(AvatarSize.XLarge).Presence(AvatarPresence.Offline)),\r\n                    SampleSubTitle(\"Persona Component\"),\r\n                    TextBlock(\"Personas combine an avatar with descriptive text.\"),\r\n                    VStack().Children(\r\n                        Persona(\"Jordan Diaz\", \"Product Designer\", \"Available for collaboration\", Avatar(initials: \"JD\").Presence(AvatarPresence.Online)),\r\n                        Persona(\"Alex Smith\", \"Software Engineer\", \"Focusing...\", Avatar(initials: \"AS\").Presence(AvatarPresence.Busy)),\r\n                        Persona(\"Kelly Lee\", \"Project Manager\", \"Away\", Avatar(initials: \"KL\").Presence(AvatarPresence.Away))\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "BadgeSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.Tags)]\r\n    public class BadgeSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public BadgeSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(BadgeSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Badges, Tags, and Chips are small visual elements used to categorize content, highlight status, or display metadata.\"),\r\n                    TextBlock(\"They come in various styles: Badges are typically static indicators, Tags are for categorization, and Chips often include interactive elements like a removal button.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use badges to call attention to small pieces of information like counts or status. Use tags for categorization where multiple labels might apply. Use chips for entities that can be removed or interacted with individually. Ensure colors are used consistently to convey meaning (e.g., red for danger/errors, green for success).\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Standard Badges\"),\r\n                    HStack().Children(\r\n                        Badge(\"Default\"),\r\n                        Badge(\"Primary\").Primary(),\r\n                        Badge(\"Success\").Success(),\r\n                        Badge(\"Warning\").Warning(),\r\n                        Badge(\"Danger\").Danger(),\r\n                        Badge(\"Info\").Info().Outline()),\r\n                    SampleSubTitle(\"Tags and Chips\"),\r\n                    TextBlock(\"Tags and chips support icons, pill shapes, and interactive removal.\"),\r\n                    HStack().Children(\r\n                        Tag(\"Categorization\").Outline().Pill(),\r\n                        Tag(\"Metadata\").SetIcon(Icon.Transform(UIcons.Tags, UIconsWeight.Regular)).Outline(),\r\n                        Chip(\"Interactive Chip\").Filled().OnRemove(c => Toast().Success(\"Removed chip\")),\r\n                        Chip(\"Status Chip\").Success().Pill())\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "ButtonSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.Cursor)]\r\n    public class ButtonSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ButtonSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ButtonSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog.\"),\r\n                    TextBlock(\"When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Buttons should clearly communicate what will happen when the user clicks them. Use concise, specific, self-explanatory labels, usually a single word. Default buttons should always perform safe operations. For example, a default button should never delete. Use only a single line of text in the label of the button. Expose only one or two buttons to the user at a time. Show only one primary button that inherits theme color at rest state. \\\"Submit\\\", \\\"OK\\\", and \\\"Apply\\\" buttons should always be styled as primary buttons.\"),\r\n                    TextBlock(\"Avoid using generic labels like \\\"Ok\\\", especially in the case of an error. Do not place the default focus on a button that destroys data. Do not use a button to navigate to another place, use a link instead.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Buttons\"),\r\n                    HStack().Children(\r\n                        Button().SetText(\"Standard\").Tooltip(\"This is a standard button\").OnClick(() => alert(\"Clicked!\")),\r\n                        Button().SetText(\"Primary\").Tooltip(\"This is a primary button\").Primary().OnClick(() => alert(\"Clicked!\")),\r\n                        Button().SetText(\"Link\").Tooltip(\"This is a link button\").Link().OnClick(() => alert(\"Clicked!\"))\r\n                    ),\r\n                    SampleSubTitle(\"Icons and States\"),\r\n                    HStack().Children(\r\n                        Button().SetText(\"Confirm\").SetIcon(UIcons.Check).Success().OnClick(() => alert(\"Clicked!\")),\r\n                        Button().SetText(\"Delete\").SetIcon(UIcons.Trash).Danger().OnClick(() => alert(\"Clicked!\")),\r\n                        Button().SetText(\"Disabled\").SetIcon(UIcons.Lock).Disabled()\r\n                    ),\r\n                    SampleSubTitle(\"Loading States\"),\r\n                    HStack().Children(\r\n                        Button().SetText(\"Click to Spin\").OnClickSpinWhile(async () => await Task.Delay(2000)),\r\n                        Button().SetText(\"With Loading Text\").OnClickSpinWhile(async () => await Task.Delay(2000), \"Processing...\"),\r\n                        Button().SetText(\"Error Simulation\").OnClickSpinWhile(async () => { await Task.Delay(1000); throw new Exception(\"Action failed\"); }, onError: (b, e) => b.SetText(\"Try again: \" + e.Message).Danger())\r\n                    ),\r\n                    SampleSubTitle(\"Variations\"),\r\n                    HStack().Children(\r\n                        ButtonAndIcon(\"Split Button\", (m, i, ev) => Toast().Information(\"Icon clicked\"), mainIcon: UIcons.Rocket, secondaryIcon: UIcons.AngleDown).OnClick((b, _) => Toast().Success(\"Main action\")),\r\n                        Button().SetText(\"No Padding\").NoPadding().Primary(),\r\n                        Button().SetText(\"No Border\").NoBorder()\r\n                    ),\r\n                    SampleSubTitle(\"Themed Backgrounds\"),\r\n                    HStack().Children(\r\n                        Button().SetText(\"Blue\").Background(Theme.Colors.Blue500).OnClick(() => alert(\"Clicked!\")),\r\n                        Button().SetText(\"Lime\").Background(Theme.Colors.Lime500).OnClick(() => alert(\"Clicked!\")),\r\n                        Button().SetText(\"Magenta\").Background(Theme.Colors.Magenta500).OnClick(() => alert(\"Clicked!\")),\r\n                        Button().SetText(\"Yellow\").Background(Theme.Colors.Yellow500).OnClick(() => alert(\"Clicked!\"))\r\n                    ),\r\n                    SampleSubTitle(\"Rounded Buttons\"),\r\n                    HStack().Children(\r\n                        Button().SetText(\"Small\").Rounded(BorderRadius.Small).Primary(),\r\n                        Button().SetText(\"Medium\").Rounded(BorderRadius.Medium).Primary(),\r\n                        Button().SetText(\"Full\").Rounded(BorderRadius.Full).Primary()\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "CarouselSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.Picture)]\r\n    public class CarouselSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public CarouselSample()\r\n        {\r\n            var textSlide1 = VStack().P(32).Children(TextBlock(\"Discover Tesserae\").Large().Bold(), TextBlock(\"Build stunning user interfaces in C# that compile to high-performance JavaScript.\")).WS();\r\n            var textSlide2 = VStack().P(32).Children(TextBlock(\"Fluent API\").Large().Bold(), TextBlock(\"Leverage a powerful, typed, and fluent API to define your layout and logic efficiently.\")).WS();\r\n            var textSlide3 = VStack().P(32).Children(TextBlock(\"Native Performance\").Large().Bold(), TextBlock(\"Zero-overhead abstractions mean your application runs fast on any modern browser.\")).WS();\r\n\r\n            var imgSlide1 = Image(\"https://cataas.com/cat\").WS().H(300).Contain();\r\n            var imgSlide2 = Image(\"https://cataas.com/cat\").WS().H(300).Contain();\r\n            var imgSlide3 = Image(\"https://cataas.com/cat\").WS().H(300).Contain();\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(CarouselSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Carousels allow users to cycle through a set of related content, such as images, features, or messages. They are effective for showcasing highlights in a limited space.\"),\r\n                    TextBlock(\"The component supports any Tesserae component as a slide and provides automatic or manual navigation.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use carousels for high-impact visual content. Keep the number of slides low (typically 3-5) to ensure users can reasonably see all content. Ensure that each slide has a clear and unique message. Provide navigation controls (arrows/dots) and ensure they are accessible. For slides with text content, ensure sufficient contrast and use .PadSlides() to prevent overlapping with controls.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Text Carousel\"),\r\n                    Carousel(textSlide1, textSlide2, textSlide3).PadSlides().H(150),\r\n                    SampleSubTitle(\"Image Gallery Carousel\"),\r\n                    Carousel(imgSlide1, imgSlide2, imgSlide3).H(300),\r\n                    SampleSubTitle(\"Interactive Carousel\"),\r\n                    Carousel(\r\n                        VStack().Children(TextBlock(\"Interactive Slide\").Medium(), Button(\"Click me\").OnClick(() => Toast().Success(\"Clicked!\"))).P(32),\r\n                        VStack().Children(TextBlock(\"Configuration Slide\").Medium(), CheckBox(\"Enable feature\")).P(32)\r\n                    ).PadSlides().H(150)\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "CheckBoxSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.Checkbox)]\r\n    public class CheckBoxSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public CheckBoxSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(CheckBoxSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"CheckBoxes allow users to select one or more items from a set. They can also be used to turn an option on or off.\"),\r\n                    TextBlock(\"Unlike a Toggle, which is typically used for immediate actions, a CheckBox is often used when a user needs to confirm their selection by clicking a submit button.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use CheckBoxes when users can select any number of options from a list. Clearly label each CheckBox so the user knows what they are selecting. If you have only two mutually exclusive options, consider using a ChoiceGroup (Radio buttons) or a Toggle. Don't use CheckBoxes as an on/off control for immediate actions; use a Toggle instead.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic CheckBoxes\"),\r\n                    VStack().Children(\r\n                        CheckBox(\"Unchecked checkbox\"),\r\n                        CheckBox(\"Checked checkbox\").Checked(),\r\n                        CheckBox(\"Disabled checkbox\").Disabled(),\r\n                        CheckBox(\"Disabled checked checkbox\").Checked().Disabled()),\r\n                    SampleSubTitle(\"Validation and States\"),\r\n                    VStack().Children(\r\n                        Label(\"Required choice\").Required().SetContent(CheckBox(\"I agree to the terms\")),\r\n                        CheckBox(\"Checkbox with tooltip\").Tooltip(\"More info here\"),\r\n                        CheckBox(\"Triggers event\").OnChange((s, e) => Toast().Information($\"Checked: {s.IsChecked}\"))),\r\n                    SampleSubTitle(\"Formatting\"),\r\n                    VStack().Children(\r\n                        CheckBox(\"Tiny\").Tiny(),\r\n                        CheckBox(\"Small (default)\").Small(),\r\n                        CheckBox(\"Small Plus\").SmallPlus(),\r\n                        CheckBox(\"Medium\").Medium(),\r\n                        CheckBox(\"Large\").Large(),\r\n                        CheckBox(\"XLarge\").XLarge(),\r\n                        CheckBox(\"XXLarge\").XXLarge(),\r\n                        CheckBox(\"Mega\").Mega(),\r\n                        CheckBox(\"Bold text\").Bold()),\r\n                    SampleSubTitle(\"Rounded CheckBoxes\"),\r\n                    VStack().Children(\r\n                        CheckBox(\"Small rounded\").Rounded(BorderRadius.Small),\r\n                        CheckBox(\"Medium rounded\").Rounded(BorderRadius.Medium),\r\n                        CheckBox(\"Full rounded\").Rounded(BorderRadius.Full)\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "ChoiceGroupSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.List)]\r\n    public class ChoiceGroupSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ChoiceGroupSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ChoiceGroupSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ChoiceGroups, also known as radio button groups, allow users to select exactly one option from a set of mutually exclusive choices.\"),\r\n                    TextBlock(\"They emphasize all options equally, which can be useful when you want to ensure the user considers all available alternatives before making a selection.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use ChoiceGroups when there are between 2 and 7 options and screen space is available. For more than 7 options, a Dropdown is typically more efficient. List options in a logical order (e.g., most likely to least likely). Align options vertically whenever possible for better readability and localization support. Always provide a default selection if one is significantly more likely than the others, and ensure the safest option is the default if applicable.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic ChoiceGroup\"),\r\n                    ChoiceGroup().Choices(\r\n                        Choice(\"Option A\"),\r\n                        Choice(\"Option B\"),\r\n                        Choice(\"Option C\").Disabled(),\r\n                        Choice(\"Option D\")),\r\n                    SampleSubTitle(\"Required with Label\"),\r\n                    ChoiceGroup(\"Select an environment\").Required().Choices(\r\n                        Choice(\"Development\"),\r\n                        Choice(\"Staging\"),\r\n                        Choice(\"Production\")),\r\n                    SampleSubTitle(\"Horizontal Layout\"),\r\n                    ChoiceGroup(\"Sizes\").Horizontal().Choices(\r\n                        Choice(\"Small\"),\r\n                        Choice(\"Medium\").Medium(),\r\n                        Choice(\"Large\").Large()),\r\n                    SampleSubTitle(\"Event Handling\"),\r\n                    ChoiceGroup(\"Language\").Choices(\r\n                        Choice(\"English\"),\r\n                        Choice(\"Spanish\"),\r\n                        Choice(\"French\")\r\n                    ).OnChange((s, e) => Toast().Information($\"Selected: {s.SelectedOption.Text}\")),\r\n                    SampleSubTitle(\"Formatting\"),\r\n                    ChoiceGroup(\"Pick a style\").Choices(\r\n                        Choice(\"Tiny\").Tiny(),\r\n                        Choice(\"Small (default)\").Small(),\r\n                        Choice(\"Small Plus\").SmallPlus(),\r\n                        Choice(\"Medium\").Medium(),\r\n                        Choice(\"Large\").Large(),\r\n                        Choice(\"XLarge\").XLarge(),\r\n                        Choice(\"XXLarge\").XXLarge(),\r\n                        Choice(\"Mega\").Mega(),\r\n                        Choice(\"Bold\").Bold()\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "ColorPickerSample": 
                            return "using static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 10, Icon = UIcons.Palette)]\r\n    public class ColorPickerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        public ColorPickerSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ColorPickerSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The ColorPicker allows users to select a color using the browser's native color selection widget. It returns the selected color as both a hex string and a Color object.\"),\r\n                    TextBlock(\"This component is useful for personalization settings, drawing applications, or any interface where color customization is required.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use the ColorPicker when users need to select a precise color that isn't covered by a predefined set of options. If you only need a few specific colors, consider using a ChoiceGroup with custom styling or a Dropdown instead. Always provide a default color that makes sense for the context. Ensure the picked color is validated if certain constraints apply (e.g., must be a dark color for text readability).\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic ColorPicker\"),\r\n                    VStack().Children(\r\n                        Label(\"Pick a color\").SetContent(\r\n                            HStack().Children(\r\n                                ColorPicker().Var(out var cp1).Width(50.px()),\r\n                                Button(\"Apply Color\").Var(out var btn1)\r\n                            )\r\n                        ),\r\n                        Label(\"With default color (Blue)\").SetContent(ColorPicker(Color.FromString(\"#0078d4\")).Width(50.px())),\r\n                        Label(\"Disabled state\").Disabled().SetContent(ColorPicker().Disabled().Width(50.px()))\r\n                    ),\r\n                    SampleSubTitle(\"Validation\"),\r\n                    VStack().Children(\r\n                        Label(\"Light color required\").SetContent(ColorPicker().Validation(Validation.LightColor).Width(50.px())),\r\n                        Label(\"Dark color required\").SetContent(ColorPicker().Validation(Validation.DarkColor).Width(50.px()))\r\n                    ),\r\n                    SampleSubTitle(\"Interactive Example\"),\r\n                    TextBlock(\"Changing the color picker below will update the button's background.\")\r\n                ));\r\n\r\n            cp1.OnChange((_, __) => btn1.Background = cp1.Text);\r\n            btn1.OnClick((_, __) => Toast().Information($\"Selected Color: {cp1.Text} (Hex: {cp1.Color.ToHex()})\"));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "CommandBarSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.MenuDots)]\r\n    public class CommandBarSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public CommandBarSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(CommandBarSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Command Bars provide a surface for common actions related to a specific context, such as a page or a selected item in a list.\"),\r\n                    TextBlock(\"They typically contain buttons with icons and labels, and can be split into 'near' items (left-aligned) and 'far' items (right-aligned).\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Command Bars for primary actions that users perform frequently. Keep the number of items manageable; if there are too many, consider using a 'More' menu. Order items by importance or frequency of use. Group related actions together. Use 'far' items for actions that are global to the surface, such as settings or search.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Command Bar\"),\r\n                    CommandBar(\r\n                        CommandBarItem(\"New\", UIcons.Plus).Primary().OnClick(() => Toast().Success(\"New item\")),\r\n                        CommandBarItem(\"Edit\", UIcons.Edit).OnClick(() => Toast().Success(\"Edit item\")),\r\n                        CommandBarItem(\"Share\", UIcons.Share).OnClick(() => Toast().Success(\"Share item\")),\r\n                        CommandBarItem(\"Delete\", UIcons.Trash).OnClick(() => Toast().Success(\"Delete item\"))\r\n                    ).FarItems(\r\n                        SearchBox().SetPlaceholder(\"Search...\").Width(200.px()),\r\n                        CommandBarItem(\"Settings\", UIcons.Settings).OnClick(() => Toast().Information(\"Settings clicked\"))\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "CronEditorSample": 
                            return "using H5;\r\nusing System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 100, Icon = UIcons.Clock)]\r\n    public sealed class CronEditorSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        public CronEditorSample()\r\n        {\r\n            _content = SectionStack()\r\n                .Title(SampleHeader(nameof(CronEditorSample)))\r\n                .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"CronEditor allows users to schedule tasks using a simplified UI for daily schedules, with a fallback to raw cron expressions for advanced users.\")\r\n                ))\r\n                .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic\"),\r\n                    CronEditor(),\r\n\r\n                    SampleSubTitle(\"With Days Selection Disabled\"),\r\n                    CronEditor().DaysEnabled(false),\r\n\r\n                    SampleSubTitle(\"Custom Interval (30 mins)\"),\r\n                    CronEditor().MinuteInterval(30),\r\n\r\n                    SampleSubTitle(\"With Initial Value (Custom)\"),\r\n                    CronEditor(\"*/5 * * * *\"),\r\n\r\n                    SampleSubTitle(\"Initially Disabled\"),\r\n                    CronEditor(initialEnabled: false),\r\n\r\n                    SampleSubTitle(\"With Enable Checkbox Hidden\"),\r\n                    CronEditor().ShowEnableCheckbox(false),\r\n\r\n                    SampleSubTitle(\"Observable\"),\r\n                    GetObservableExample()\r\n                ));\r\n        }\r\n\r\n        private IComponent GetObservableExample()\r\n        {\r\n            var editor = CronEditor();\r\n            var text = TextBlock(\"Current value: \" + editor.Value.cron + \" (\" + (editor.Value.enabled ? \"Enabled\" : \"Disabled\") + \")\");\r\n            editor.OnChange((s) => text.Text = \"Current value: \" + s.Value.cron + \" (\" + (s.Value.enabled ? \"Enabled\" : \"Disabled\") + \")\");\r\n            return VStack().Children(editor, text);\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "DatePickerSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 10, Icon = UIcons.Calendar)]\r\n    public class DatePickerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public DatePickerSample()\r\n        {\r\n            var from = DateTime.Now.AddDays(-7);\r\n            var to   = DateTime.Now.AddDays(7);\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(DatePickerSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"DatePickers allow users to select a specific date using a browser-native date selection widget. They ensure that the input is always a valid date format.\"),\r\n                    TextBlock(\"This component is suitable for forms requiring birthdays, appointment dates, or any date-driven data entry.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use the DatePicker when users need to enter a specific date. If you need to include time as well, use the DateTimePicker instead. Always provide min and max constraints if the acceptable date range is limited. Use clear validation messages to guide users if they select an invalid date (e.g., a date in the past when only future dates are allowed).\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic DatePicker\"),\r\n                    VStack().Children(\r\n                        Label(\"Standard\").SetContent(DatePicker()),\r\n                        Label(\"Pre-selected Date (Next Week)\").SetContent(DatePicker(DateTime.Now.AddDays(7))),\r\n                        Label(\"Disabled\").Disabled().SetContent(DatePicker().Disabled())\r\n                    ),\r\n                    SampleSubTitle(\"Range and Constraints\"),\r\n                    VStack().Children(\r\n                        Label($\"Limited Range (Between {from:d} and {to:d})\").SetContent(DatePicker().SetMin(from).SetMax(to)),\r\n                        Label(\"Step increment of 5 days\").SetContent(DatePicker().SetStep(5))\r\n                    ),\r\n                    SampleSubTitle(\"Validation\"),\r\n                    VStack().Children(\r\n                        Label(\"Not in the future\").SetContent(DatePicker().Validation(Validation.NotInTheFuture)),\r\n                        Label(\"Not in the past\").SetContent(DatePicker().Validation(Validation.NotInThePast)),\r\n                        Label(\"Custom validation (within 2 months)\").SetContent(DatePicker().Validation(dp => dp.Date <= DateTime.Now.AddMonths(2) ? null : \"Please choose a date less than 2 months in the future\"))\r\n                    ),\r\n                    SampleSubTitle(\"Event Handling\"),\r\n                    DatePicker().OnChange((s, e) => Toast().Information($\"Selected date: {s.Date:d}\"))\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "DateTimePickerSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 10, Icon = UIcons.CalendarClock)]\r\n    public class DateTimePickerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public DateTimePickerSample()\r\n        {\r\n            var from = DateTime.Now.AddDays(-7);\r\n            var to   = DateTime.Now.AddDays(7);\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(DateTimePickerSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The DateTimePicker combines date and time selection into a single component, using the browser's native widget.\"),\r\n                    TextBlock(\"It is ideal for scheduling events, setting deadlines, or any task where both the day and time are critical.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use the DateTimePicker when users need to specify a precise moment in time. Consider the user's timezone if the application handles users across different regions. Provide sensible defaults, such as the current time or a common starting point. Use min/max constraints to prevent invalid selections (e.g., booking an appointment in the past).\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic DateTimePicker\"),\r\n                    VStack().Children(\r\n                        Label(\"Standard\").SetContent(DateTimePicker()),\r\n                        Label(\"Pre-selected (Now)\").SetContent(DateTimePicker(DateTime.Now)),\r\n                        Label(\"Disabled\").Disabled().SetContent(DateTimePicker().Disabled())\r\n                    ),\r\n                    SampleSubTitle(\"Constraints\"),\r\n                    VStack().Children(\r\n                        Label($\"Range: {from:g} to {to:g}\").SetContent(DateTimePicker().SetMin(from).SetMax(to)),\r\n                        Label(\"10-second intervals\").SetContent(DateTimePicker().SetStep(10))\r\n                    ),\r\n                    SampleSubTitle(\"Validation\"),\r\n                    VStack().Children(\r\n                        Label(\"Must be in the future\").SetContent(DateTimePicker().Validation(Validation.NotInThePast)),\r\n                        Label(\"Within next 48 hours\").SetContent(DateTimePicker().Validation(dtp => dtp.DateTime <= DateTime.Now.AddHours(48) ? null : \"Must be within 48 hours\"))\r\n                    ),\r\n                    SampleSubTitle(\"Event Handling\"),\r\n                    DateTimePicker().OnChange((s, e) => Toast().Information($\"Selected: {s.DateTime:g}\"))\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "DeltaComponent": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing System.Linq;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 100, Icon = UIcons.Refresh)]\r\n    public class DeltaComponentSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public DeltaComponentSample()\r\n        {\r\n            var deltaContainer = document.createElement(\"div\");\r\n            var deltaComponent = DeltaComponent(Raw(deltaContainer)).Animated();\r\n\r\n            var html = \"\";\r\n            int step = 1;\r\n\r\n            var typing = Button(\"Type Lorem Ipsum\").OnClick(() =>\r\n            {\r\n                var lorem = \"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris  nisi ut aliquip ex ea commodo consequat.\";\r\n\r\n                var d1 = document.createElement(\"div\");\r\n                d1.innerHTML = \"<div><span></span><b>Starting...</b></div>\";\r\n                deltaComponent.ReplaceContent(Raw(d1));\r\n\r\n                int index = 0;\r\n\r\n                void TypeNextChar()\r\n                {\r\n                    if (index > lorem.Length)\r\n                    {\r\n                        var dFinal = document.createElement(\"div\");\r\n                        dFinal.innerHTML = $\"<div><span>{lorem}</span><b> Done \u2714</b></div>\";\r\n                        deltaComponent.ReplaceContent(Raw(dFinal));\r\n                        return;\r\n                    }\r\n\r\n                    var currentText = lorem.Substring(0, index);\r\n\r\n                    var d = document.createElement(\"div\");\r\n                    d.innerHTML = $\"<div><span>{currentText}</span><b>Typing... {index}/{lorem.Length}</b></div>\";\r\n                    deltaComponent.ReplaceContent(Raw(d));\r\n\r\n                    index++;\r\n                    window.setTimeout(_ => TypeNextChar(), 25);\r\n                }\r\n\r\n                TypeNextChar();\r\n            });\r\n\r\n            var typingWithComponents = Button(\"Type Lorem Ipsum 2\").OnClick(() =>\r\n            {\r\n                var lorem = \"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris  nisi ut aliquip ex ea commodo consequat.\".ToArray();\r\n\r\n                var stack = HStack().WS().Children(TextBlock(\"Starting...\"));\r\n\r\n                deltaComponent.ReplaceContent(stack);\r\n\r\n                int index = 0;\r\n\r\n                void TypeNextChar()\r\n                {\r\n                    if (index > lorem.Length)\r\n                    {\r\n                        stack = HStack().WS().Children(lorem.Select(t => TextBlock(t.ToString()).PR(t == ' ' ? 4 : 0)).ToArray(), Icon(UIcons.Check).PR(8));\r\n                        deltaComponent.ReplaceContent(stack);\r\n                        return;\r\n                    }\r\n\r\n                    var currentText = lorem.Take(index).ToArray();\r\n                    stack = HStack().WS().Children(currentText.Select(t => TextBlock(t.ToString()).PR(t == ' ' ? 4 : 0)).ToArray());\r\n                    deltaComponent.ReplaceContent(stack);\r\n\r\n                    index++;\r\n                    window.setTimeout(_ => TypeNextChar(), 25);\r\n                }\r\n\r\n                TypeNextChar();\r\n            });\r\n\r\n            var resetBtn = Button(\"Reset\").OnClick(() =>\r\n            {\r\n                html = \"\";\r\n                step = 1;\r\n                var d = document.createElement(\"div\");\r\n                deltaComponent.ReplaceContent(Raw(d));\r\n            });\r\n\r\n            // Shadow DOM Sample\r\n            var shadowContainer = document.createElement(\"div\");\r\n            var shadowDeltaComponent = DeltaComponent(Raw(shadowContainer), useShadowDom: true).Animated();\r\n\r\n            var shadowTyping = Button(\"Type in Shadow DOM\").OnClick(() =>\r\n            {\r\n                var lorem = \"This text is inside a Shadow DOM!\";\r\n\r\n                var d1 = document.createElement(\"div\");\r\n                d1.innerHTML = \"<div><span></span><b>Shadow Starting...</b></div>\";\r\n                shadowDeltaComponent.ReplaceContent(Raw(d1));\r\n\r\n                int index = 0;\r\n\r\n                void TypeNextChar()\r\n                {\r\n                    if (index > lorem.Length)\r\n                    {\r\n                        var dFinal = document.createElement(\"div\");\r\n                        dFinal.innerHTML = $\"<div><span>{lorem}</span><b> Shadow Done \u2714</b></div>\";\r\n                        shadowDeltaComponent.ReplaceContent(Raw(dFinal));\r\n                        return;\r\n                    }\r\n\r\n                    var currentText = lorem.Substring(0, index);\r\n\r\n                    var d = document.createElement(\"div\");\r\n                    d.innerHTML = $\"<div><span>{currentText}</span><b>Shadow Typing... {index}/{lorem.Length}</b></div>\";\r\n                    shadowDeltaComponent.ReplaceContent(Raw(d));\r\n\r\n                    index++;\r\n                    window.setTimeout(_ => TypeNextChar(), 25);\r\n                }\r\n\r\n                TypeNextChar();\r\n            });\r\n\r\n             var shadowResetBtn = Button(\"Reset Shadow\").OnClick(() =>\r\n            {\r\n                var d = document.createElement(\"div\");\r\n                d.textContent = \"Shadow DOM Initial Content\";\r\n                shadowDeltaComponent.ReplaceContent(Raw(d));\r\n            });\r\n\r\n\r\n            _content = SectionStack()\r\n                .Title(SampleHeader(nameof(DeltaComponent)))\r\n                .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"DeltaComponent updates its DOM tree to match a new component's DOM tree using a diff algorithm. It detects text appends and adds them as new spans to avoid full re-rendering.\"),\r\n                    HStack().Children(typing, typingWithComponents, resetBtn),\r\n                    SampleTitle(\"Output\"),\r\n                    deltaComponent,\r\n                    SampleTitle(\"Shadow DOM\"),\r\n                    TextBlock(\"This DeltaComponent renders its content inside a Shadow DOM root.\"),\r\n                    HStack().Children(shadowTyping, shadowResetBtn),\r\n                    shadowDeltaComponent\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "DropdownSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.CaretSquareDown)]\r\n    public sealed class DropdownSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        public DropdownSample()\r\n        {\r\n            var validatedDropdown = Dropdown().Items(\r\n                            DropdownItem(\"Option 1\"),\r\n                            DropdownItem(\"Option 2\")\r\n                        );\r\n            validatedDropdown.Attach(dd => dd.IsInvalid = dd.SelectedItems.Length != 1 || dd.SelectedItems[0].Text != \"Option 1\");\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(DropdownSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"A Dropdown is a list in which the selected item is always visible, and the others are visible on demand by clicking a drop-down button.\"),\r\n                    TextBlock(\"They are used to simplify the design and make a choice within the UI. When closed, only the selected item is visible. When users click the drop-down button, all the options become visible.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use a Dropdown when there are multiple choices that can be collapsed under one title, especially if the list of items is long or when space is constrained. Use shortened statements or single words as options. Dropdowns are preferred over radio buttons when the selected option is more important than the alternatives. For less than 7 options, consider using a ChoiceGroup if space allows.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Dropdown\"),\r\n                    VStack().Children(\r\n                        Label(\"Standard\").SetContent(Dropdown().Items(\r\n                            DropdownItem(\"Option 1\").Selected(),\r\n                            DropdownItem(\"Option 2\"),\r\n                            DropdownItem(\"Option 3\")\r\n                        )),\r\n                        Label(\"With Headers and Dividers\").SetContent(Dropdown().Items(\r\n                            DropdownItem(\"Group 1\").Header(),\r\n                            DropdownItem(\"Item 1.1\"),\r\n                            DropdownItem(\"Item 1.2\"),\r\n                            DropdownItem().Divider(),\r\n                            DropdownItem(\"Group 2\").Header(),\r\n                            DropdownItem(\"Item 2.1\"),\r\n                            DropdownItem(\"Item 2.2\").Selected()\r\n                        ))\r\n                    ),\r\n                    SampleSubTitle(\"Selection Modes\"),\r\n                    VStack().Children(\r\n                        Label(\"Multi-select\").SetContent(Dropdown().Multi().Items(\r\n                            DropdownItem(\"Apple\"),\r\n                            DropdownItem(\"Banana\").Selected(),\r\n                            DropdownItem(\"Orange\").Selected(),\r\n                            DropdownItem(\"Grape\")\r\n                        )),\r\n                        Label(\"Custom Arrow Icon\").SetContent(Dropdown().SetArrowIcon(UIcons.AnglesUpDown).Items(\r\n                            DropdownItem(\"Low\"),\r\n                            DropdownItem(\"Medium\").Selected(),\r\n                            DropdownItem(\"High\")\r\n                        ))\r\n                    ),\r\n                    SampleSubTitle(\"Async Loading\"),\r\n                    VStack().Children(\r\n                        Label(\"Load on open (5s delay)\").SetContent(Dropdown().Items(GetItemsAsync)),\r\n                        Label(\"Load immediately (5s delay)\").SetContent(StartLoadingAsyncDataImmediately(Dropdown().Items(GetItemsAsync))),\r\n                        Label(\"Empty State\").SetContent(Dropdown(\"No items available\").Items(new Dropdown.Item[0]))\r\n                    ),\r\n                    SampleSubTitle(\"Validation\"),\r\n                    VStack().Children(\r\n                        Label(\"Required Dropdown\").SetContent(Dropdown().Required().Items(\r\n                            DropdownItem(\"Choose one...\").Header(),\r\n                            DropdownItem(\"Valid Choice\")\r\n                        )),\r\n                        Label(\"Validation (Must select 'Option 1')\").SetContent(validatedDropdown)\r\n                    ),\r\n                    SampleSubTitle(\"Rounded Dropdowns\"),\r\n                    VStack().Children(\r\n                        Label(\"Small\").SetContent(Dropdown().Rounded(BorderRadius.Small).Items(DropdownItem(\"Option 1\"), DropdownItem(\"Option 2\"))),\r\n                        Label(\"Medium\").SetContent(Dropdown().Rounded(BorderRadius.Medium).Items(DropdownItem(\"Option 1\"), DropdownItem(\"Option 2\"))),\r\n                        Label(\"Full\").SetContent(Dropdown().Rounded(BorderRadius.Full).Items(DropdownItem(\"Option 1\"), DropdownItem(\"Option 2\")))\r\n                    )\r\n                ));\r\n        }\r\n\r\n        private static Dropdown StartLoadingAsyncDataImmediately(Dropdown dropdown)\r\n        {\r\n            dropdown.LoadItemsAsync().FireAndForget();\r\n            return dropdown;\r\n        }\r\n\r\n        private async Task<Dropdown.Item[]> GetItemsAsync()\r\n        {\r\n            await Task.Delay(5000);\r\n            return new[]\r\n            {\r\n                DropdownItem(\"Header 1\").Header(),\r\n                DropdownItem(\"Async Item 1\"),\r\n                DropdownItem(\"Async Item 2\"),\r\n                DropdownItem().Divider(),\r\n                DropdownItem(\"Header 2\").Header(),\r\n                DropdownItem(\"Async Item 3\")\r\n            };\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "EditableLabelSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 20, Icon = UIcons.Edit)]\r\n    public class EditableLabelSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public EditableLabelSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(EditableLabelSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"EditableLabels and EditableAreas allow users to view content as standard text and switch to an editing mode (input or textarea) upon interaction.\"),\r\n                    TextBlock(\"They are useful for 'in-place' editing where you want to keep the UI clean but allow users to quickly modify specific fields without navigating to a separate form.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use EditableLabels for short, single-line content like titles or names. Use EditableAreas for longer, multi-line content like descriptions. Always provide an OnSave() callback to persist the changes. Ensure the interaction to trigger editing is clear\u2014typically by showing an edit icon on hover or using a distinct visual style. Consider using validation to ensure the entered data meets your requirements.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Editable Labels\"),\r\n                    VStack().Children(\r\n                        EditableLabel(\"Click to edit this text\"),\r\n                        EditableLabel(\"Large and Bold Title\").Large().Bold(),\r\n                        EditableLabel(\"Pre-configured font size\").MediumPlus()\r\n                    ),\r\n                    SampleSubTitle(\"Editable Area\"),\r\n                    TextBlock(\"For multi-line text input:\"),\r\n                    EditableArea(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Click here to edit the entire block of text.\").Width(400.px()),\r\n                    SampleSubTitle(\"Events and Validation\"),\r\n                    VStack().Children(\r\n                        EditableLabel(\"Change me and check the toast\")\r\n                           .OnSave((s, text) => { Toast().Success($\"Saved: {text}\"); return true; }),\r\n                        Label(\"Required Field\").Required().SetContent(EditableLabel(\"Can't be empty\"))\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "GridPickerSample": 
                            return "using System;\r\nusing System.Linq;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 10, Icon = UIcons.TableLayout)]\r\n    public class GridPickerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        public GridPickerSample()\r\n        {\r\n            var picker = GridPicker(\r\n                columnNames: new[] { \"Monday\", \"Tuesday\", \"Wednesday\", \"Thursday\", \"Friday\", \"Saturday\", \"Sunday\" },\r\n                rowNames: new[] { \"Morning\", \"Afternoon\", \"Night\" },\r\n                states: 3,\r\n                initialStates: new[]\r\n                {\r\n                    new[] { 0, 0, 0, 0, 0, 0, 0 },\r\n                    new[] { 0, 0, 0, 0, 0, 0, 0 },\r\n                    new[] { 0, 0, 0, 0, 0, 0, 0 }\r\n                },\r\n                formatState: (btn, state, previousState) =>\r\n                {\r\n                    string text = \"\";\r\n                    switch (state)\r\n                    {\r\n                        case 0: text = \"\u2620\"; break;\r\n                        case 1: text = \"\ud83d\udc22\"; break;\r\n                        case 2: text = \"\ud83d\udc07\"; break;\r\n                    }\r\n\r\n                    if (previousState >= 0 && previousState != state)\r\n                    {\r\n                        switch (previousState)\r\n                        {\r\n                            case 0: text = $\"\u2620 -> {text}\"; break;\r\n                            case 1: text = $\"\ud83d\udc22 -> {text}\"; break;\r\n                            case 2: text = $\"\ud83d\udc07 -> {text}\"; break;\r\n                        }\r\n                    }\r\n                    btn.SetText(text);\r\n                });\r\n\r\n            var hourPicker = GridPicker(\r\n                rowNames: new[] { \"Monday\", \"Tuesday\", \"Wednesday\", \"Thursday\", \"Friday\", \"Saturday\", \"Sunday\" },\r\n                columnNames: Enumerable.Range(0, 24).Select(n => $\"{n:00}\").ToArray(),\r\n                states: 4,\r\n                initialStates: Enumerable.Range(0, 7).Select(_ => new int[24]).ToArray(),\r\n                formatState: (btn, state, previousState) =>\r\n                {\r\n                    string color = \"\";\r\n                    switch (state)\r\n                    {\r\n                        case 0: color = \"#c7c5c5\"; break;\r\n                        case 1: color = \"#a3cfa5\"; break;\r\n                        case 2: color = \"#76cc79\"; break;\r\n                        case 3: color = \"#1fcc24\"; break;\r\n                    }\r\n                    btn.Background(color);\r\n                },\r\n                columns: new[] { 128.px(), 24.px() },\r\n                rowHeight: 24.px());\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(GridPickerSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The GridPicker component provides an interactive grid where each cell can cycle through a predefined number of states. It's highly customizable through its state formatting logic.\"),\r\n                    TextBlock(\"Common use cases include scheduling, availability maps, or any scenario where you need to visualize and edit state across two dimensions.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use GridPickers for dense state selection where labels for each cell would be too cluttered. Provide a clear legend or visual cues for what each state represents. Ensure the row and column headers are descriptive. If the grid is large, consider how it will behave on smaller screens. Leverage the state formatting to provide rich feedback, such as changing colors, icons, or text based on the current state.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Simple Schedule Example\"),\r\n                    TextBlock(\"Click on cells to cycle through states: Dead (\u2620), Slow (\ud83d\udc22), and Fast (\ud83d\udc07).\"),\r\n                    picker,\r\n                    SampleSubTitle(\"Heatmap/Availability Example\"),\r\n                    TextBlock(\"Assigning different background colors based on state levels (0 to 3).\"),\r\n                    hourPicker,\r\n                    SampleSubTitle(\"Dynamic/Calculated Grid\"),\r\n                    TextBlock(\"Using GridPicker for a complex logic visualization (Game of Life).\"),\r\n                    GetGameOfLifeSample()));\r\n        }\r\n\r\n        private static Stack GetGameOfLifeSample()\r\n        {\r\n            int  height   = 32;\r\n            int  width    = 32;\r\n            bool isPaused = false;\r\n\r\n            var grid = GridPicker(\r\n                rowNames: Enumerable.Range(0,    height).Select(n => $\"{n:00}\").ToArray(),\r\n                columnNames: Enumerable.Range(0, width).Select(n => $\"{n:00}\").ToArray(),\r\n                states: 2,\r\n                initialStates: Enumerable.Range(0, width).Select(_ => new int[height]).ToArray(),\r\n                formatState: (btn, state, previousState) =>\r\n                {\r\n                    string color = state == 0 ? Theme.Default.Background : Theme.Default.Foreground;\r\n                    btn.Background(color);\r\n                },\r\n                columns: new[] { 128.px(), 24.px() },\r\n                rowHeight: 24.px());\r\n\r\n            grid.WhenMounted(() =>\r\n            {\r\n                var t = window.setInterval((_) => { if (grid.IsMounted()) Grow(); }, 200);\r\n                grid.WhenRemoved(() => window.clearInterval(t));\r\n            });\r\n\r\n            var btnReset = Button(\"Reset\").SetIcon(UIcons.Bomb).OnClick(() =>\r\n            {\r\n                var state = grid.GetState();\r\n                foreach (var a in state) for (int i = 0; i < a.Length; i++) a[i] = 0;\r\n                grid.SetState(state);\r\n            });\r\n\r\n            var btnPause = Button(\"Pause\").SetIcon(UIcons.Pause);\r\n            btnPause.OnClick(() =>\r\n            {\r\n                isPaused = !isPaused;\r\n                btnPause.SetIcon(isPaused ? UIcons.Play : UIcons.Pause);\r\n                btnPause.SetText(isPaused ? \"Resume\" : \"Pause\");\r\n            });\r\n\r\n            void Grow()\r\n            {\r\n                if (grid.IsDragging || isPaused) return;\r\n                var previous = grid.GetState();\r\n                var cells    = grid.GetState();\r\n                for (int i = 0; i < height; i++)\r\n                {\r\n                    for (int j = 0; j < width; j++)\r\n                    {\r\n                        int alive = GetNeighbors(previous, i, j);\r\n                        if (cells[i][j] == 1) cells[i][j] = (alive < 2 || alive > 3) ? 0 : 1;\r\n                        else if (alive == 3) cells[i][j] = 1;\r\n                    }\r\n                }\r\n                grid.SetState(cells);\r\n            }\r\n\r\n            int GetNeighbors(int[][] cells, int x, int y)\r\n            {\r\n                int count = 0;\r\n                for (int i = x - 1; i <= x + 1; i++)\r\n                {\r\n                    for (int j = y - 1; j <= y + 1; j++)\r\n                    {\r\n                        if (i < 0 || j < 0 || i >= height || j >= width || (i == x && j == y)) continue;\r\n                        if (cells[i][j] == 1) count++;\r\n                    }\r\n                }\r\n                return count;\r\n            }\r\n\r\n            return VStack().WS().Children(HStack().WS().Children(btnPause, btnReset), grid.WS());\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "GridSample": 
                            return "using System;\r\nusing System.Linq;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 10, Icon = UIcons.Grid)]\r\n    public class GridSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        public GridSample()\r\n        {\r\n            var grid = Grid(columns: new[] { 1.fr(), 1.fr(), 200.px() });\r\n            grid.Gap(8.px());\r\n            grid.Add(Button().SetText(\"Stretched Item\").WS().Primary().GridColumnStretch().GridRow(1, 2));\r\n            Enumerable.Range(1, 10).ForEach(v => grid.Add(Button().SetText($\"Item {v}\")));\r\n\r\n            var gridAutoSize = Grid(new UnitSize(\"repeat(auto-fit, minmax(min(200px, 100%), 1fr))\"));\r\n            gridAutoSize.Gap(8.px());\r\n            Enumerable.Range(1, 10).ForEach(v => gridAutoSize.Add(Card(TextBlock($\"Responsive Item {v}\").TextCenter())));\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(GridSample)))\r\n               .Section(VStack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The Grid component provides a powerful layout system based on CSS Grid. It allows you to define columns, rows, and gaps between items.\"),\r\n                    TextBlock(\"Items within a Grid can be explicitly positioned or stretched across multiple tracks, offering full control over complex 2D layouts.\")))\r\n               .Section(VStack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Grid for page-level layouts or complex component structures where both rows and columns need coordination. For simple one-dimensional layouts (horizontal or vertical), consider using HStack or VStack instead. Leverage 'fr' units for flexible columns that fill available space proportionally. Use 'auto-fit' or 'auto-fill' with 'minmax' to create responsive grids that adapt to different screen sizes without media queries.\")))\r\n               .Section(VStack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Fixed and Flexible Columns\"),\r\n                    TextBlock(\"This grid uses two flexible columns (1fr) and one fixed column (200px). The first item is stretched across all columns.\"),\r\n                    grid,\r\n                    SampleSubTitle(\"Responsive Auto-fit Grid\"),\r\n                    TextBlock(\"This grid automatically adjusts the number of columns based on the available width (min 200px per item).\"),\r\n                    gridAutoSize\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "HorizontalSeparatorSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 20, Icon = UIcons.HorizontalRule)]\r\n    public class HorizontalSeparatorSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public HorizontalSeparatorSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(HorizontalSeparatorSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"A HorizontalSeparator visually divides content into groups. It can optionally contain text or other components to label the group it introduces.\"),\r\n                    TextBlock(\"The content can be aligned to the left, center, or right of the line.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use separators to provide structure to long forms or pages. Keep labels short and concise. Use them sparingly; too many separators can clutter the UI. Ensure the labels accurately describe the section that follows.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Text Alignment\"),\r\n                    HorizontalSeparator(\"Center Aligned (Default)\"),\r\n                    HorizontalSeparator(\"Left Aligned\").Left(),\r\n                    HorizontalSeparator(\"Right Aligned\").Right(),\r\n                    SampleSubTitle(\"Themed and Custom Content\"),\r\n                    VStack().Children(\r\n                        HorizontalSeparator(\"Primary Color\").Primary(),\r\n                        HorizontalSeparator(HStack().Children(\r\n                            Icon(UIcons.Info).PaddingRight(8.px()),\r\n                            TextBlock(\"Information Section\").SemiBold()\r\n                        )).Primary().Left()\r\n                    ),\r\n                    SampleSubTitle(\"Empty Separator\"),\r\n                    TextBlock(\"A simple line without any label:\"),\r\n                    HorizontalSeparator(\"\")\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "LabelSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.TextSize)]\r\n    public class LabelSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public LabelSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(LabelSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Labels provide a name or title for a component or a group of components. They are essential for accessibility and helping users understand the purpose of input fields.\"),\r\n                    TextBlock(\"While many Tesserae components have built-in labels, the standalone Label component offers more flexibility in positioning and styling.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use sentence casing for label text. Keep labels short and concise, typically using a noun or a short noun phrase. Do not use labels as instructional text; use TextBlocks or tooltips for that purpose. Ensure labels are positioned close to the components they describe. Use the 'Required' flag to clearly indicate mandatory fields.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Labels\"),\r\n                    VStack().Children(\r\n                        Label(\"Standard Label\"),\r\n                        Label(\"Required Label\").Required(),\r\n                        Label(\"Disabled Label\").Disabled(),\r\n                        Label(\"Primary Colored Label\").Primary()\r\n                    ),\r\n                    SampleSubTitle(\"Label with Content\"),\r\n                    Label(\"Username\").SetContent(TextBox().SetPlaceholder(\"Enter your username\")),\r\n                    SampleSubTitle(\"Inline Layouts\"),\r\n                    TextBlock(\"Labels can be displayed inline with their content, with optional automatic width synchronization.\"),\r\n                    VStack().Children(\r\n                        Label(\"Name\").Inline().AutoWidth().SetContent(TextBox()),\r\n                        Label(\"Department\").Inline().AutoWidth().SetContent(TextBox()),\r\n                        Label(\"Role\").Inline().AutoWidth().SetContent(TextBox())\r\n                    ),\r\n                    SampleSubTitle(\"Right Aligned Labels\"),\r\n                    VStack().Children(\r\n                        Label(\"Short\").Inline().AutoWidth(alignRight: true).SetContent(TextBox()),\r\n                        Label(\"A much longer label\").Inline().AutoWidth(alignRight: true).SetContent(TextBox())\r\n                    ),\r\n                    SampleSubTitle(\"Rounded Labels\"),\r\n                    VStack().Children(\r\n                        Label(\"Small rounded\").Rounded(BorderRadius.Small),\r\n                        Label(\"Medium rounded\").Rounded(BorderRadius.Medium),\r\n                        Label(\"Full rounded\").Rounded(BorderRadius.Full)\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "MessageSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.Comment)]\r\n    public class MessageSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public MessageSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(MessageSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The Message component is used to display static messages, alerts, or empty states. It supports an icon, title, text body, and an optional note area.\"),\r\n                    TextBlock(\"It comes with variants for standard, success, warning, and error states.\")\r\n               ))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Standard Message (with Note)\"),\r\n                    Message(\"No Database Schema Yet\", \"Start by describing your database requirements in the chat. I'll help you design a complete schema with tables, relationships, and best practices.\")\r\n                        .Icon(UIcons.FileCode)\r\n                        .Note(\r\n                            HStack().AlignItems(ItemAlign.Center).Children(\r\n                                Icon(UIcons.Bulb, size: TextSize.Small).PR(8),\r\n                                TextBlock(\"Try saying: \\\"Create a blog database with users, posts, and comments\\\"\").SemiBold()\r\n                            )\r\n                        ),\r\n\r\n                    SampleSubTitle(\"Error Message\"),\r\n                    Message(\"Something went wrong\", \"We couldn't save your changes. Please check your internet connection and try again.\")\r\n                        .Icon(UIcons.CrossCircle)\r\n                        .Variant(MessageVariant.Error),\r\n\r\n                    SampleSubTitle(\"No Results\"),\r\n                    Message(\"No results found\", \"We couldn't find any items matching your search criteria.\")\r\n                        .Icon(UIcons.Search)\r\n                        .Variant(MessageVariant.Default)\r\n               ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "NavbarSample": 
                            return "using System;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.MenuDots)]\r\n    public class NavbarSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public NavbarSample()\r\n        {\r\n            var navbar = Sidebar().AsNavbar();\r\n\r\n            navbar.AddHeader(new SidebarButton(\"brand\", UIcons.Rocket, \"My App\").Primary());\r\n            navbar.AddHeader(new SidebarButton(\"dashboard\", UIcons.Dashboard, \"Dashboard\"));\r\n\r\n            navbar.AddContent(new SidebarButton(\"profile\", UIcons.User, \"Profile\"));\r\n            navbar.AddContent(new SidebarButton(\"settings\", UIcons.Settings, \"Settings\"));\r\n            navbar.AddContent(new SidebarSeparator(\"sep1\"));\r\n            navbar.AddContent(new SidebarButton(\"logout\", UIcons.SignOutAlt, \"Logout\"));\r\n\r\n            navbar.AddFooter(new SidebarButton(\"footer\", UIcons.Info, \"About\"));\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(NavbarSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"A Sidebar rendered as a Navbar. Header items are inline, others are in a drawer.\"),\r\n                    SampleTitle(\"Usage\"),\r\n                    VStack().H(500.px()).Children(\r\n                        navbar,\r\n                        TextBlock(\"Page Content below the navbar...\").Padding(16.px())\r\n                    )\r\n               ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "NumberPickerSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 10, Icon = UIcons.InputNumeric)]\r\n    public class NumberPickerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public NumberPickerSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(NumberPickerSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The NumberPicker provides an input field specifically for numeric values, leveraging the browser's native number input widget.\"),\r\n                    TextBlock(\"It supports constraints like minimum and maximum values, as well as step increments for easier value adjustment.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use the NumberPicker whenever a precise numeric input is required. Set appropriate 'min', 'max', and 'step' values to guide the user. If the range of numbers is small and discrete, consider using a Slider or ChoiceGroup instead. Use validation to ensure the entered number meets specific criteria (e.g., must be even or positive).\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic NumberPickers\"),\r\n                    VStack().Children(\r\n                        Label(\"Standard\").SetContent(NumberPicker()),\r\n                        Label(\"Initial value (42)\").SetContent(NumberPicker(42)),\r\n                        Label(\"Disabled\").Disabled().SetContent(NumberPicker().Disabled())\r\n                    ),\r\n                    SampleSubTitle(\"Constraints\"),\r\n                    VStack().Children(\r\n                        Label(\"Between 0 and 100\").SetContent(NumberPicker().SetMin(0).SetMax(100)),\r\n                        Label(\"Step increment of 5\").SetContent(NumberPicker().SetStep(5))\r\n                    ),\r\n                    SampleSubTitle(\"Validation\"),\r\n                    VStack().Children(\r\n                        Label(\"Must be an even number\").SetContent(NumberPicker().Validation(np => np.Value % 2 == 0 ? null : \"Please choose an even value\")),\r\n                        Label(\"Required field\").SetContent(NumberPicker().Required())\r\n                    ),\r\n                    SampleSubTitle(\"Event Handling\"),\r\n                    NumberPicker().OnChange((s, e) => Toast().Information($\"Value changed to: {s.Value}\"))\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "PaginationSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.List)]\r\n    public class PaginationSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public PaginationSample()\r\n        {\r\n            var status = TextBlock(\"Showing page 1\").Medium();\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(PaginationSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Pagination allows users to navigate through a large set of data by breaking it into smaller, manageable chunks called pages.\"),\r\n                    TextBlock(\"It provides controls to move between pages, jump to specific pages, and see the current position within the total set.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use pagination when you have a large amount of content that would be overwhelming or slow to load all at once. Clearly show the total number of items and the current page. Provide 'Previous' and 'Next' controls for sequential navigation. If the number of pages is high, consider using a simplified view or allowing the user to jump to the first/last page. Keep the pagination controls in a consistent location, typically at the bottom of the content area.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Pagination\"),\r\n                    Card(status).MB(16),\r\n                    Pagination(totalItems: 120, pageSize: 10, currentPage: 1)\r\n                       .OnPageChange(p => status.Text = $\"Showing page {p.CurrentPage}\"),\r\n                    SampleSubTitle(\"Small Result Set\"),\r\n                    Pagination(totalItems: 25, pageSize: 10, currentPage: 1)\r\n                       .OnPageChange(p => Toast().Information($\"Selected page {p.CurrentPage}\")),\r\n                    SampleSubTitle(\"Large Result Set\"),\r\n                    Pagination(totalItems: 1000, pageSize: 20, currentPage: 5)\r\n                       .OnPageChange(p => Toast().Information($\"Selected page {p.CurrentPage}\"))\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "PickerSample": 
                            return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 10, Icon = UIcons.CaretSquareDown)]\r\n    public class PickerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public PickerSample()\r\n        {\r\n            _content = SectionStack()\r\n                   .Title(SampleHeader(nameof(PickerSample)))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Overview\"),\r\n                        TextBlock(\"Pickers are used to select one or more items, such as people or tags, from a large list. They provide a search-based interface with suggestions.\"),\r\n                        TextBlock(\"This component is highly flexible, allowing for custom item rendering, single or multiple selections, and different suggestion behaviors.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Best Practices\"),\r\n                        TextBlock(\"Use Pickers when the number of options is too large for a standard Dropdown. Ensure that the items can be easily searched by text. Use clear icons or visual indicators if it helps users identify the correct item quickly. For multiple selections, consider how the selected items will be displayed\u2014either inline or in a separate list. Provide a helpful 'suggestions title' to guide the user when they interact with the picker.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        SampleSubTitle(\"Multi-selection Picker\"),\r\n                        TextBlock(\"Allows selecting multiple tags from the suggestions.\"),\r\n                        Picker<PickerSampleItem>(suggestionsTitleText: \"Suggested Names\").Items(GetPickerItems()).MB(32),\r\n                        SampleSubTitle(\"Single Selection Picker\"),\r\n                        TextBlock(\"Limits selection to only one item at a time.\"),\r\n                        Picker<PickerSampleItem>(suggestionsTitleText: \"Select one\", maximumAllowedSelections: 1).Items(GetPickerItems()).MB(32),\r\n                        SampleSubTitle(\"Custom Rendered Items\"),\r\n                        TextBlock(\"Using icons and complex components for both suggestions and selections.\"),\r\n                        Picker<PickerSampleItemWithComponents>(suggestionsTitleText: \"System Items\", renderSelectionsInline: false).Items(GetComponentPickerItems())\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n\r\n        private PickerSampleItem[] GetPickerItems()\r\n        {\r\n            return new[]\r\n            {\r\n                new PickerSampleItem(\"Bob\"),\r\n                new PickerSampleItem(\"BOB\"),\r\n                new PickerSampleItem(\"Donuts by J Dilla\"),\r\n                new PickerSampleItem(\"Donuts\"),\r\n                new PickerSampleItem(\"Coffee\"),\r\n                new PickerSampleItem(\"Chicken Coop\"),\r\n                new PickerSampleItem(\"Cherry Pie\"),\r\n                new PickerSampleItem(\"Chess\"),\r\n                new PickerSampleItem(\"Cooper\")\r\n            };\r\n        }\r\n\r\n        private PickerSampleItemWithComponents[] GetComponentPickerItems()\r\n        {\r\n            return new[]\r\n            {\r\n                new PickerSampleItemWithComponents(\"Bob\", UIcons.Bomb),\r\n                new PickerSampleItemWithComponents(\"BOB\", UIcons.BlenderPhone),\r\n                new PickerSampleItemWithComponents(\"Donuts\", UIcons.Carrot),\r\n                new PickerSampleItemWithComponents(\"Coffee\", UIcons.Coffee),\r\n                new PickerSampleItemWithComponents(\"Chess\", UIcons.Chess),\r\n                new PickerSampleItemWithComponents(\"Cooper\", UIcons.Interrogation)\r\n            };\r\n        }\r\n    }\r\n\r\n    public class PickerSampleItem : IPickerItem\r\n    {\r\n        public PickerSampleItem(string name) => Name = name;\r\n        public string Name { get; }\r\n        public bool IsSelected { get; set; }\r\n        public IComponent Render() => TextBlock(Name);\r\n    }\r\n\r\n    public class PickerSampleItemWithComponents : IPickerItem\r\n    {\r\n        private readonly UIcons _icon;\r\n        public PickerSampleItemWithComponents(string name, UIcons icon) { Name = name; _icon = icon; }\r\n        public string Name { get; }\r\n        public bool IsSelected { get; set; }\r\n        public IComponent Render() => HStack().AlignContent(ItemAlign.Center).Children(Icon(_icon).MinWidth(16.px()), TextBlock(Name).ML(8));\r\n    }\r\n}\r\n";
                        case "SaveButtonSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 100, Icon = UIcons.Disk)]\r\n    public class SaveButtonSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SaveButtonSample()\r\n        {\r\n            var saveButton = SaveButton().Pending().OnClick(async () => {\r\n                // Demo logic inside the click\r\n            });\r\n\r\n            // For manual state control\r\n            var manualButton = SaveButton().NothingToSave();\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SaveButtonSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The SaveButton component is a wrapper around a Button that manages common saving states: Pending, Verifying, Saving, Saved, and Error.\")\r\n               ))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Manual State Control\"),\r\n                    HStack().Children(\r\n                        manualButton,\r\n                        Stack().Children(\r\n                            Button(\"Set Nothing to Save\"  ).OnClick(() => manualButton.NothingToSave()),\r\n                            Button(\"Set Pending\"          ).OnClick(() => manualButton.Pending()),\r\n                            Button(\"Set Verifying\"        ).OnClick(() => manualButton.Verifying()),\r\n                            Button(\"Set Saving\"           ).OnClick(() => manualButton.Saving()),\r\n                            Button(\"Set Saved\"            ).OnClick(() => manualButton.Saved()),\r\n                            Button(\"Set Error\"            ).OnClick(() => manualButton.Error(\"Validation failed!\"))\r\n                        ).Gap(8.px())\r\n                    ).Gap(16.px()),\r\n\r\n                    SampleSubTitle(\"Live Demo\"),\r\n                    TextBlock(\"Click the button below to simulate a save operation.\"),\r\n                    saveButton.OnClick(async () => {\r\n                        saveButton.Verifying();\r\n                        await Task.Delay(1000);\r\n                        saveButton.Saving();\r\n                        await Task.Delay(2000);\r\n                        saveButton.Saved();\r\n                        await Task.Delay(2000);\r\n                        saveButton.NothingToSave();\r\n                        await Task.Delay(2000);\r\n                        saveButton.Pending();\r\n                    })\r\n                ))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Hover State\"),\r\n                    TextBlock(\"This SaveButton has a hover text configured. Hover over it when it is in Pending state.\"),\r\n                    SaveButton().Configure(save: \"Disabled\", saveHover: \"Enable Now!\", saveIcon: UIcons.ToggleOff   , saveHoverIcon: UIcons.ToggleOn).Pending()\r\n               ))\r\n               .Section(Stack().Children(\r\n                   SampleTitle(\"Dynamic Text Update\"),\r\n                   TextBlock(\"This SaveButton text can be updated dynamically.\"),\r\n                   DynamicTextUpdateSample()\r\n              ));\r\n        }\r\n\r\n        private IComponent DynamicTextUpdateSample()\r\n        {\r\n            var btn = SaveButton().Pending();\r\n            return HStack().Children(\r\n                btn,\r\n                Stack().Children(\r\n                    Button(\"Update Save Text\").OnClick(() => btn.Configure(save: \"New Save Text\")),\r\n                    Button(\"Update Hover Text\").OnClick(() => btn.Configure(saveHover: \"New Hover Text\"))\r\n                ).Gap(8.px())\r\n            ).Gap(16.px());\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SavingToastSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 20, Icon = UIcons.Bell)]\r\n    public class SavingToastSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SavingToastSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SavingToastSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"The SavingToast component helps viewing the state of a saving operation (Saving, Saved, Error) with appropriate icons and colors.\")\r\n               ))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    HStack().Children(\r\n                        Button(\"Trigger Saving\").OnClick(() => SavingToast().Saving(\"Saving data...\")),\r\n                        Button(\"Trigger Saved\" ).OnClick(() => SavingToast().Saved(\"Data saved successfully!\")),\r\n                        Button(\"Trigger Error\" ).OnClick(() => SavingToast().Error(\"Could not save data.\")),\r\n                        Button(\"Trigger Error with Close\" ).OnClick(() => SavingToast().Error(\"Could not save data.\", untilDismissed: true)),\r\n                        Button(\"Many Toasts\").OnClickSpinWhile(() => ShowMany())\r\n                    ).Gap(8.px()),\r\n\r\n                    SampleSubTitle(\"Live Demo\"),\r\n                    Button(\"Simulate Save Process\").OnClick(async () => {\r\n                        var savingToast = SavingToast();\r\n                        savingToast.Saving(\"Starting save...\");\r\n                        await Task.Delay(2000);\r\n                        savingToast.Saved(\"All done!\");\r\n                    })\r\n                ));\r\n        }\r\n\r\n        private async Task ShowMany()\r\n        {\r\n            var toast1 = SavingToast();\r\n            var toast2 = SavingToast();\r\n            var toast3 = SavingToast();\r\n            toast1.Saving(\"Starting save...\");\r\n            toast2.Saving(\"Starting save...\");\r\n            toast3.Saving(\"Starting save...\");\r\n            await Task.Delay(2000);\r\n            toast1.Saved(\"All done!\");\r\n            await Task.Delay(2000);\r\n            toast2.Saved(\"All done!\");\r\n            await Task.Delay(2000);\r\n            toast3.Saved(\"All done!\");\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SearchBoxSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 20, Icon = UIcons.Search)]\r\n    public class SearchBoxSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SearchBoxSample()\r\n        {\r\n            var searchAsYouType = TextBlock(\"Start typing in the 'Search as you type' box below...\");\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SearchBoxSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"SearchBoxes provide an input field for searching through content, allowing users to locate specific items within the website or app.\"),\r\n                    TextBlock(\"They include a search icon and a clear button, and support both 'on search' (e.g., when Enter is pressed) and 'search as you type' behaviors.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Always use placeholder text to describe the search scope (e.g., 'Search files'). Use the 'Underlined' style for CommandBars or other minimalist surfaces. Enable 'Search as you type' for small to medium datasets where results can be filtered instantly. Provide a clear visual cue when no results are found. Don't use a SearchBox if you cannot reliably provide accurate results.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic SearchBoxes\"),\r\n                    VStack().Children(\r\n                        Label(\"Default Search\").SetContent(SearchBox(\"Search...\").OnSearch((s, e) => Toast().Information($\"Searched for: {e}\"))),\r\n                        Label(\"Underlined\").SetContent(SearchBox(\"Search site\").Underlined().OnSearch((s, e) => Toast().Information($\"Searched for: {e}\"))),\r\n                        Label(\"Disabled\").Disabled().SetContent(SearchBox(\"Search disabled\").Disabled())\r\n                    ),\r\n                    SampleSubTitle(\"Search Behaviors\"),\r\n                    VStack().Children(\r\n                        Label(\"Search as you type\").SetContent(\r\n                            SearchBox(\"Type something...\")\r\n                                .SearchAsYouType()\r\n                                .OnSearch((s, e) => searchAsYouType.Text = string.IsNullOrEmpty(e) ? \"Waiting for input...\" : $\"Current search: {e}\")\r\n                        ),\r\n                        searchAsYouType\r\n                    ),\r\n                    SampleSubTitle(\"Customization\"),\r\n                    VStack().Children(\r\n                        Label(\"Custom Icon (Filter)\").SetContent(SearchBox(\"Filter items...\").SetIcon(UIcons.Filter)),\r\n                        Label(\"No Icon\").SetContent(SearchBox(\"Iconless search\").NoIcon()),\r\n                        Label(\"Fixed Width (250px)\").SetContent(SearchBox(\"Small search\").Width(250.px()))\r\n                    ),\r\n                    SampleSubTitle(\"Rounded SearchBoxes\"),\r\n                    VStack().Children(\r\n                        SearchBox(\"Small\").Rounded(BorderRadius.Small),\r\n                        SearchBox(\"Medium\").Rounded(BorderRadius.Medium),\r\n                        SearchBox(\"Full\").Rounded(BorderRadius.Full)\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SidebarSample": 
                            return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 100, Icon = UIcons.MenuDots)]\r\n    public class SidebarSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SidebarSample()\r\n        {\r\n            var sidebar = Sidebar();\r\n\r\n            var searchBox = new SidebarSearchBox(\"search\", \"Search Sidebar...\");\r\n            searchBox.OnSearch((term) => sidebar.Search(term));\r\n\r\n            sidebar.AddHeader(searchBox);\r\n\r\n            sidebar.AddContent(new SidebarButton(\"home\", UIcons.Home, \"Home\"));\r\n            sidebar.AddContent(new SidebarButton(\"profile\", UIcons.User, \"Profile\"));\r\n\r\n            sidebar.AddContent(new SidebarSeparator(\"sep1\", \"Grouping\"));\r\n\r\n            var settingsNav = new SidebarNav(\"settings\", UIcons.Settings, \"Settings\", true);\r\n            settingsNav.Add(new SidebarButton(\"general\", UIcons.Settings, \"General\"));\r\n            settingsNav.Add(new SidebarButton(\"security\", UIcons.Lock, \"Security\"));\r\n            settingsNav.Add(new SidebarButton(\"privacy\", UIcons.Eye, \"Privacy\"));\r\n\r\n            sidebar.AddContent(settingsNav);\r\n\r\n            sidebar.AddContent(new SidebarSeparator(\"sep2\"));\r\n\r\n            sidebar.AddContent(new SidebarButton(\"help\", UIcons.Question, \"Help\"));\r\n\r\n            // --- Moved from App.cs ---\r\n\r\n            var lightDark = new SidebarCommand(UIcons.Sun).Tooltip(\"Light Mode\");\r\n\r\n            lightDark.OnClick(() =>\r\n            {\r\n                if (Theme.IsDark)\r\n                {\r\n                    Theme.Light();\r\n                    lightDark.SetIcon(UIcons.Sun).Tooltip(\"Light Mode\");\r\n                }\r\n                else\r\n                {\r\n                    Theme.Dark();\r\n                    lightDark.SetIcon(UIcons.Moon).Tooltip(\"Dark Mode\");\r\n                }\r\n            });\r\n\r\n            var toast  = new SidebarCommand(Emoji.Bread).Tooltip(\"Toast !\").OnClick(() => Toast().Success(\"Here is your toast \ud83c\udf5e\"));\r\n            var pizza  = new SidebarCommand(Emoji.Pizza).Tooltip(\"Pizza!\").OnClick(() => Toast().Success(\"Here is your pizza \ud83c\udf55\"));\r\n            var cheese = new SidebarCommand(Emoji.Cheese).Tooltip(\"Cheese !\").OnClick(() => Toast().Success(\"Here is your cheese \ud83e\uddc0\"));\r\n\r\n            var commands = new SidebarCommands(\"TOASTS\", lightDark, toast, pizza, cheese);\r\n\r\n\r\n            var fireworks = new SidebarCommand(Emoji.ConfettiBall).Tooltip(\"Confetti !\").OnClick(() => Toast().Success(\"\ud83c\udf8a\"));\r\n            var happy     = new SidebarCommand(Emoji.Smile).Tooltip(\"I like this !\").OnClick(() => Toast().Success(\"Thanks for your feedback\"));\r\n            var sad       = new SidebarCommand(Emoji.Disappointed).Tooltip(\"I don't like this!\").OnClick(() => Toast().Success(\"Thanks for your feedback\"));\r\n\r\n            var dotsMenu = new SidebarCommand(UIcons.MenuDots).OnClickMenu(() => new ISidebarItem[]\r\n            {\r\n                new SidebarButton(\"MANAGE_ACCOUNT\", UIcons.User,     \"Manage Account\"),\r\n                new SidebarButton(\"PREFERENCES\",    UIcons.Settings, \"Preferences\"),\r\n                new SidebarButton(\"DELETE\",         UIcons.Trash,    \"Delete Account\"),\r\n                new SidebarCommands(\"EMOTIONS\", new SidebarCommand(Emoji.Smile), new SidebarCommand(Emoji.Disappointed), new SidebarCommand(Emoji.Angry)),\r\n                new SidebarCommands(\"ADD_DELETE\", new SidebarCommand(UIcons.Plus).Primary(), new SidebarCommand(UIcons.Trash).Danger()).AlignEnd(),\r\n                new SidebarButton(\"SIGNOUT\", UIcons.SignOutAlt, \"Sign Out\"),\r\n            });\r\n\r\n            var commandsEndAligned = new SidebarCommands(\"SETTINGS\", fireworks, dotsMenu).AlignEnd();\r\n\r\n            sidebar.AddFooter(new SidebarNav(\"DEEP_NAV\", Emoji.EvergreenTree, \"Multi-Depth Nav\", true).Sortable(sortableGroup: \"trees\").AddRange(CreateDeepNav(\"root\")));\r\n\r\n            sidebar.AddFooter(new SidebarNav(\"EMPTY_NAV\", Emoji.MailboxWithNoMail, \"Empty Nav\", true).ShowDotIfEmpty().OnOpenIconClick((e, m) => Toast().Success(\"You clicked on the icon!\")));\r\n\r\n\r\n            sidebar.AddFooter(commands);\r\n            sidebar.AddFooter(commandsEndAligned);\r\n\r\n            sidebar.AddFooter(new SidebarButton(\"CURIOSITY_REF\",\r\n                new ImageIcon(\"/assets/img/curiosity-logo.svg\"),\r\n                \"By Curiosity\",\r\n                new SidebarBadge(\"+3\").Foreground(Theme.Primary.Foreground).Background(Theme.Primary.Background),\r\n                new SidebarCommand(UIcons.ArrowUpRightFromSquare).OnClick(() => window.open(\"https://github.com/curiosity-ai/tesserae\", \"_blank\"))).Tooltip(\"Made with \u2764 by Curiosity\").OnClick(() => window.open(\"https://curiosity.ai\", \"_blank\")));\r\n\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SidebarSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"A fully featured Sidebar with Search, Navigation, Buttons, and Separators.\"),\r\n                    SampleTitle(\"Usage\"),\r\n                    Stack().Children(\r\n                        sidebar.S().H(800.px())\r\n                    )\r\n               ));\r\n        }\r\n\r\n        private static IEnumerable<ISidebarItem> CreateDeepNav(string path, int currentDepth = 0, int maxDepth = 3)\r\n        {\r\n            if (currentDepth < maxDepth)\r\n            {\r\n                Action<SidebarNav.ParentChangedEvent> HandleChange = (e)=>\r\n                {\r\n                    Dialog($\"Move element {e.Item.OwnIdentifier} from {e.From.OwnIdentifier} to {e.To.OwnIdentifier}?\").YesNo(onNo: e.Cancel);\r\n                };\r\n                yield return new SidebarNav($\"{path}/{currentDepth + 1}.1\", Emoji.DeciduousTree, $\"{path}/{currentDepth + 1}.1\", true).Sortable(sortableGroup: \"trees\").AddRange(CreateDeepNav($\"{path}/{currentDepth + 1}.1\", currentDepth + 1, maxDepth)).OnParentChanged(HandleChange);\r\n                yield return new SidebarNav($\"{path}/{currentDepth + 1}.2\", Emoji.DeciduousTree, $\"{path}/{currentDepth + 1}.2\", true).Sortable(sortableGroup: \"trees\").AddRange(CreateDeepNav($\"{path}/{currentDepth + 1}.2\", currentDepth + 1, maxDepth)).OnParentChanged(HandleChange);\r\n                yield return new SidebarNav($\"{path}/{currentDepth + 1}.3\", Emoji.DeciduousTree, $\"{path}/{currentDepth + 1}.3\", true).Sortable(sortableGroup: \"trees\").AddRange(CreateDeepNav($\"{path}/{currentDepth + 1}.3\", currentDepth + 1, maxDepth)).OnParentChanged(HandleChange);\r\n            }\r\n        }\r\n\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SidebarSeparatorSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 99, Icon = UIcons.MenuDots)]\r\n    public class SidebarSeparatorSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SidebarSeparatorSample()\r\n        {\r\n            var sidebar = Sidebar();\r\n\r\n            sidebar.AddHeader(new SidebarText(\"header\", \"Header\"));\r\n            sidebar.AddContent(new SidebarButton(\"1\", UIcons.Home, \"Home\"));\r\n            sidebar.AddContent(new SidebarSeparator(\"sep1\"));\r\n            sidebar.AddContent(new SidebarButton(\"2\", UIcons.User, \"Profile\"));\r\n            sidebar.AddContent(new SidebarSeparator(\"sep2\", \"More Options\"));\r\n            sidebar.AddContent(new SidebarButton(\"3\", UIcons.Settings, \"Settings\"));\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SidebarSeparatorSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"A separator for the Sidebar component to visually group items.\"),\r\n                    SampleTitle(\"Usage\"),\r\n                    TextBlock(\"Basic separator:\"),\r\n                    Stack().Children(\r\n                        sidebar.S().H(500.px())\r\n                    )\r\n               ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SkeletonSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.Spinner)]\r\n    public class SkeletonSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SkeletonSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SkeletonSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Skeleton loaders are used to provide a placeholder for content that is still loading. They help reduce the perceived load time and prevent layout shifts by reserving the space that the final content will occupy.\"),\r\n                    TextBlock(\"They come in various shapes like circles for avatars and rectangles for text or images.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use skeleton loaders when content takes more than a second to load. Match the shape and size of the skeleton as closely as possible to the actual content it replaces. Avoid using skeletons for very fast-loading content as it can cause flickering. Ensure the skeleton's color and animation are subtle and fit with the overall theme.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Avatar and Text Placeholder\"),\r\n                    HStack().Children(\r\n                        Skeleton(SkeletonType.Circle).W(48).H(48),\r\n                        VStack().Children(\r\n                            Skeleton().W(200).H(16).ML(16).MB(8),\r\n                            Skeleton().W(140).H(12).ML(16))),\r\n                    SampleSubTitle(\"Article/Image Placeholder\"),\r\n                    VStack().Children(\r\n                        Skeleton(SkeletonType.Rect).WS().H(200),\r\n                        Skeleton().WS().H(16).MT(16),\r\n                        Skeleton().W(80.percent()).H(16).MT(8),\r\n                        Skeleton().W(60.percent()).H(16).MT(8)\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SliderSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.SettingsSliders)]\r\n    public class SliderSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SliderSample()\r\n        {\r\n            var value = new SettableObservable<int>(50);\r\n            var s1    = Slider(val: 50, min: 0, max: 100, step: 1).OnInput((s,  e) => value.Value = s.Value);\r\n            var s2    = Slider(val: 20, min: 0, max: 100, step: 10).OnInput((s, e) => Toast().Information($\"Value changed to {s.Value}\"));\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SliderSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Sliders allow users to select a value from a continuous or discrete range of values by moving a thumb along a track.\"),\r\n                    TextBlock(\"They are ideal for settings that don't require high precision but benefit from a visual representation of the available range, such as volume or brightness.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use sliders when users need to choose a value from a range where the relative position is more important than the exact value. Provide clear labels for the minimum and maximum values. If the user needs to select a precise number, consider using a NumberPicker alongside or instead of a slider. Use discrete steps (increments) if the available choices are limited to specific intervals.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Sliders\"),\r\n                    VStack().Children(\r\n                        Label(\"Continuous Slider (step: 1)\").SetContent(s1),\r\n                        HStack().Children(TextBlock(\"Current Value: \"), DeferSync(value, v => TextBlock(v.ToString()).SemiBold())),\r\n                        Label(\"Discrete Slider (step: 10)\").SetContent(s2)\r\n                    ),\r\n                    SampleSubTitle(\"States\"),\r\n                    VStack().Children(\r\n                        Label(\"Disabled Slider\").Disabled().SetContent(Slider(val: 30).Disabled()),\r\n                        Label(\"Required Slider\").Required().SetContent(Slider(val: 10))\r\n                    )\r\n                    //TODO: Fix vertical sliders:\r\n                    //SampleSubTitle(\"Vertical Sliders\"),\r\n                    //HStack().Children(\r\n                    //    Slider(val: 50).Vertical().H(150),\r\n                    //    Slider(val: 20).Vertical().H(150),\r\n                    //    Slider(val: 80).Vertical().H(150)\r\n                    //)\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "SplitViewSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.TableColumns)]\r\n    public class SplitViewSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SplitViewSample()\r\n        {\r\n            var splitView     = SplitView().Left(Stack().S().Background(Theme.Colors.Neutral100).Children(TextBlock(\"Left Panel\").Bold().AlignCenter())).Right(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock(\"Right Panel\").Bold().AlignCenter())).Resizable().WS().H(200);\r\n            var horzSplitView = HorizontalSplitView().Top(Stack().S().Background(Theme.Colors.Neutral100).Children(TextBlock(\"Top Panel\").Bold().AlignCenter())).Bottom(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock(\"Bottom Panel\").Bold().AlignCenter())).Resizable().WS().H(200);\r\n\r\n            _content = SectionStack()\r\n               .S()\r\n               .Title(SampleHeader(nameof(SplitViewSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"SplitViews divide a surface into two areas, either horizontally or vertically. They are commonly used for master-detail layouts, navigation sidebars, or resizable workspace areas.\"),\r\n                    TextBlock(\"Tesserae provides both 'SplitView' (vertical split) and 'HorizontalSplitView' with support for resizable handles and initial sizing.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use SplitViews when users need to see two related sets of content at the same time. Enable resizability if the ideal balance between the two panels depends on the user's task or screen size. Set sensible minimum and maximum sizes for the panels to prevent them from disappearing or becoming too large. Use distinct background colors or borders to help users distinguish between the two areas.\")))\r\n               .Section(\r\n                    VStack().S().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        SampleSubTitle(\"Interaction Controls\"),\r\n                        HStack().WS().Children(\r\n                            Button(\"Make Non-Resizable\").OnClick(() => { splitView.NotResizable(); horzSplitView.NotResizable(); Toast().Information(\"Resizing disabled\"); }),\r\n                            Button(\"Make Resizable\").Primary().OnClick(() => { splitView.Resizable(); horzSplitView.Resizable(); Toast().Information(\"Resizing enabled\"); })\r\n                        ).MB(16),\r\n                        SampleSubTitle(\"Vertical SplitView\"),\r\n                        splitView.MB(16),\r\n                        SampleSubTitle(\"Horizontal SplitView\"),\r\n                        horzSplitView\r\n                    )\r\n                  , grow: true);\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "StepperSample": 
                            return "using static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 30, Icon = UIcons.ListCheck)]\r\n    public class StepperSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public StepperSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(StepperSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Steppers (also known as Wizards) guide users through a multi-step process by breaking it down into smaller, logical chunks.\"),\r\n                    TextBlock(\"They manage the visibility of content for each step and provide built-in navigation controls (Previous/Next) while tracking the current progress.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Steppers for complex tasks that have a clear sequential order. Keep each step focused on a single topic to avoid overwhelming the user. Provide clear labels for each step so the user knows what to expect. Use the 'Review' step to allow users to verify their input before the final submission. Ensure that the 'Previous' action allows users to return and modify their entries without losing data.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Registration Wizard\"),\r\n                    Stepper(\r\n                        Step(\"Personal Info\", Stack().Children(\r\n                            TextBlock(\"Tell us about yourself:\").MB(16),\r\n                            Label(\"Full Name\").SetContent(TextBox().SetPlaceholder(\"John Doe\")),\r\n                            Label(\"Email Address\").SetContent(TextBox().SetPlaceholder(\"john@example.com\"))\r\n                        )),\r\n                        Step(\"Preferences\", Stack().Children(\r\n                            TextBlock(\"Customize your experience:\").MB(16),\r\n                            Toggle(onText: TextBlock(\"Yes\"), offText: TextBlock(\"No\")).Checked(),\r\n                            Toggle(onText: TextBlock(\"Dark\"), offText: TextBlock(\"Light\")),\r\n                            Label(\"Favorite Color\").SetContent(ColorPicker())\r\n                        )),\r\n                        Step(\"Terms & Review\", Stack().Children(\r\n                            TextBlock(\"Please review and accept our terms:\").MB(16),\r\n                            Card(TextBlock(\"Detailed terms and conditions text goes here...\").Small()),\r\n                            Label(\"Acceptance\").Required().SetContent(CheckBox(\"I agree to the terms of service\")),\r\n                            Button(\"Complete Registration\").Primary().MT(16)\r\n                        ))\r\n                    ).OnStepChange(s => Toast().Information($\"Step {s.CurrentStepIndex + 1}: {s.CurrentStep.Title}\"))\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "TextBlockSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.F)]\r\n    public class TextBlockSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public TextBlockSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(TextBlockSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"TextBlock is the fundamental component for displaying text in Tesserae. It provides a consistent way to apply typography styles, sizes, and weights across your application.\"),\r\n                    TextBlock(\"It supports various built-in sizes, from tiny to mega, and different weights and colors.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use the predefined text sizes to maintain visual hierarchy. Use semi-bold or bold weights for headers and important information. Leverage the built-in color options (primary, success, danger, etc.) to convey meaning consistently. For long blocks of text, ensure the width is constrained for better readability. Use 'NoWrap' and text-overflow properties when dealing with limited space, such as in list items.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Text Sizes\"),\r\n                    VStack().Children(\r\n                        TextBlock(\"Mega Text\").Mega(),\r\n                        TextBlock(\"XXLarge Text\").XXLarge(),\r\n                        TextBlock(\"XLarge Text\").XLarge(),\r\n                        TextBlock(\"Large Text\").Large(),\r\n                        TextBlock(\"MediumPlus Text\").MediumPlus(),\r\n                        TextBlock(\"Medium Text (Default)\").Medium(),\r\n                        TextBlock(\"SmallPlus Text\").SmallPlus(),\r\n                        TextBlock(\"Small Text\").Small(),\r\n                        TextBlock(\"XSmall Text\").XSmall(),\r\n                        TextBlock(\"Tiny Text\").Tiny()\r\n                    ),\r\n                    SampleSubTitle(\"Weights and Colors\"),\r\n                    VStack().Children(\r\n                        TextBlock(\"Bold Primary Text\").Bold().Primary(),\r\n                        TextBlock(\"Semi-Bold Success Text\").SemiBold().Success(),\r\n                        TextBlock(\"Regular Danger Text\").Regular().Danger()\r\n                    ),\r\n                    SampleSubTitle(\"Wrapping and Overflow\"),\r\n                    VStack().Children(\r\n                        TextBlock(\"Default wrapping:\").SemiBold(),\r\n                        TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.\").Width(300.px()),\r\n                        TextBlock(\"No wrapping (ellipsis):\").SemiBold().MT(16),\r\n                        TextBlock(\"This is a very long text that will be truncated with an ellipsis because it has NoWrap set and a constrained width.\").NoWrap().Width(300.px())\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "TextBoxSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.InputText)]\r\n    public class TextBoxSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public TextBoxSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(TextBoxSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"TextBoxes allow users to enter and edit text. They are used in forms, search queries, and anywhere text input is required.\"),\r\n                    TextBlock(\"They support various modes like password input, read-only states, and built-in validation.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Always label your TextBoxes so users know what information is expected. Use placeholder text to provide a hint about the format or content. Mark required fields clearly. Use validation to provide immediate feedback on the correctness of the input. Use the appropriate input type (e.g., Password) for sensitive information. Provide a clear way to submit or clear the data.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic TextBoxes\"),\r\n                    VStack().Children(\r\n                        Label(\"Standard\").SetContent(TextBox()),\r\n                        Label(\"Placeholder\").SetContent(TextBox().SetPlaceholder(\"Enter your name...\")),\r\n                        Label(\"Password\").SetContent(TextBox().Password()),\r\n                        Label(\"Disabled\").Disabled().SetContent(TextBox(\"Disabled content\").Disabled()),\r\n                        Label(\"Read-only\").SetContent(TextBox(\"Read-only content\").ReadOnly())\r\n                    ),\r\n                    SampleSubTitle(\"Validation\"),\r\n                    VStack().Children(\r\n                        Label(\"Required\").Required().SetContent(TextBox()),\r\n                        Label(\"Must not be empty\").SetContent(TextBox().Validation(tb => string.IsNullOrWhiteSpace(tb.Text) ? \"This field is required\" : null)),\r\n                        Label(\"Positive Integer only\").SetContent(TextBox().Validation(Validation.NonZeroPositiveInteger)),\r\n                        Label(\"Custom Error\").SetContent(TextBox().Error(\"Something went wrong\").IsInvalid())\r\n                    ),\r\n                    SampleSubTitle(\"Event Handling\"),\r\n                    VStack().Children(\r\n                        TextBox().SetPlaceholder(\"Type and check toast...\").OnChange((s, e) => Toast().Information($\"Text changed to: {s.Text}\")),\r\n                        TextBox().SetPlaceholder(\"Search-like behavior...\").OnInput((s, e) => console.log($\"Current input: {s.Text}\"))\r\n                    ),\r\n                    SampleSubTitle(\"Rounded TextBoxes\"),\r\n                    VStack().Children(\r\n                        TextBox().SetPlaceholder(\"Small\").Rounded(BorderRadius.Small),\r\n                        TextBox().SetPlaceholder(\"Medium\").Rounded(BorderRadius.Medium),\r\n                        TextBox().SetPlaceholder(\"Full\").Rounded(BorderRadius.Full)\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "TextBreadcrumbsSample": 
                            return "using static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 20, Icon = UIcons.MenuDots)]\r\n    public class TextBreadcrumbsSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public TextBreadcrumbsSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(TextBreadcrumbsSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"TextBreadcrumbs are a navigational aid that indicates the current position within a hierarchy. They allow users to understand their context and easily navigate back to higher-level pages.\"),\r\n                    TextBlock(\"This component is typically placed at the top of a page, below the main navigation.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use breadcrumbs for applications with deep hierarchical structures. Place them consistently at the top of the content area. Use short, descriptive labels for each level. The last item in the breadcrumb should represent the current page and is typically not clickable. Breadcrumbs should complement, not replace, the primary navigation system.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Breadcrumbs\"),\r\n                    TextBreadcrumbs().Items(\r\n                        TextBreadcrumb(\"Home\").OnClick((s, e) => Toast().Information(\"Home clicked\")),\r\n                        TextBreadcrumb(\"Settings\").OnClick((s, e) => Toast().Information(\"Settings clicked\")),\r\n                        TextBreadcrumb(\"Profile\")\r\n                    ).PB(32),\r\n                    SampleSubTitle(\"Overflow and Collapsing\"),\r\n                    TextBlock(\"When the breadcrumbs exceed the available width, they automatically collapse into an overflow menu.\"),\r\n                    TextBreadcrumbs().MaxWidth(300.px()).Items(\r\n                        TextBreadcrumb(\"Root\").OnClick((s, e) => Toast().Information(\"Root\")),\r\n                        TextBreadcrumb(\"Folder 1\").OnClick((s, e) => Toast().Information(\"Folder 1\")),\r\n                        TextBreadcrumb(\"Folder 2\").OnClick((s, e) => Toast().Information(\"Folder 2\")),\r\n                        TextBreadcrumb(\"Subfolder A\").OnClick((s, e) => Toast().Information(\"Subfolder A\")),\r\n                        TextBreadcrumb(\"Subfolder B\").OnClick((s, e) => Toast().Information(\"Subfolder B\")),\r\n                        TextBreadcrumb(\"Current File\")\r\n                    ).PB(32),\r\n                    SampleSubTitle(\"Long Breadcrumb List\"),\r\n                    TextBreadcrumbs().Items(\r\n                        TextBreadcrumb(\"Resources\"),\r\n                        TextBreadcrumb(\"Images\"),\r\n                        TextBreadcrumb(\"Icons\"),\r\n                        TextBreadcrumb(\"UIcons\"),\r\n                        TextBreadcrumb(\"Regular\"),\r\n                        TextBreadcrumb(\"Arrows\"),\r\n                        TextBreadcrumb(\"Chevron-Down.png\")\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "ToggleSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Components\", Order = 0, Icon = UIcons.SettingsSliders)]\r\n    public class ToggleSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ToggleSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ToggleSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"A Toggle represents a physical switch that allows users to choose between two mutually exclusive options, typically 'on' and 'off'.\"),\r\n                    TextBlock(\"Unlike a Checkbox, a Toggle is intended for immediate actions where the change takes effect as soon as the switch is flipped.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Toggles for binary settings that have an immediate effect (e.g., turning Wi-Fi on or off). Labels should be short and describe the setting clearly. Avoid using Toggles when a user needs to click a 'Submit' or 'Apply' button to save changes; use a Checkbox instead. Ensure that the 'on' and 'off' states are visually distinct and easy to understand at a glance.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic Toggles\"),\r\n                    VStack().Children(\r\n                        Label(\"Default (Unchecked)\").SetContent(Toggle()),\r\n                        Label(\"Checked\").SetContent(Toggle().Checked()),\r\n                        Label(\"Disabled Checked\").Disabled().SetContent(Toggle().Checked().Disabled()),\r\n                        Label(\"Disabled Unchecked\").Disabled().SetContent(Toggle().Disabled())\r\n                    ),\r\n                    SampleSubTitle(\"Custom Labels and Inline\"),\r\n                    VStack().Children(\r\n                        Toggle().SetText(\"With Label\"),\r\n                        Toggle(onText: TextBlock(\"Visible\"), offText: TextBlock(\"Hidden\")),\r\n                        Label(\"Inline Toggle\").Inline().SetContent(Toggle())\r\n                    ),\r\n                    SampleSubTitle(\"Event Handling\"),\r\n                    Toggle().SetText(\"Feature X\").OnChange((s, e) => Toast().Information($\"Feature X is now {(s.IsChecked ? \"Enabled\" : \"Disabled\")}\")),\r\n                    SampleSubTitle(\"Formatting\"),\r\n                    VStack().Children(\r\n                        Toggle(\"Tiny\").Tiny(),\r\n                        Toggle(\"Small (default)\").Small(),\r\n                        Toggle(\"Small Plus\").SmallPlus(),\r\n                        Toggle(\"Medium\").Medium(),\r\n                        Toggle(\"Large\").Large(),\r\n                        Toggle(\"XLarge\").XLarge(),\r\n                        Toggle(\"XXLarge\").XXLarge(),\r\n                        Toggle(\"Mega\").Mega(),\r\n                        Toggle(\"Bold text\").Bold()),\r\n                    SampleSubTitle(\"Rounded Toggles\"),\r\n                    VStack().Children(\r\n                        Label(\"Small\").SetContent(Toggle().Rounded(BorderRadius.Small)),\r\n                        Label(\"Medium\").SetContent(Toggle().Rounded(BorderRadius.Medium)),\r\n                        Label(\"Full\").SetContent(Toggle().Rounded(BorderRadius.Full))\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "ProgressIndicatorSample": 
                            return "using System;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Progress\", Order = 10, Icon = UIcons.Barcode)]\r\n    public class ProgressIndicatorSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ProgressIndicatorSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ProgressIndicatorSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ProgressIndicators provide visual feedback for operations that take more than a few seconds. They show the current completion status and help set expectations for how much work remains. If the total amount of work is unknown, use the indeterminate state or a Spinner instead.\")))\r\n               .Section(Stack().WidthStretch().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use a ProgressIndicator when the total units to completion can be quantified. Provide a clear label describing the operation in progress. Use the indeterminate state only when the duration is unknown. Combine multiple related steps into a single progress bar for a smoother experience. Avoid letting progress appear to move backwards unless a step failed and is being retried.\")))\r\n               .Section(\r\n                    Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        TextBlock(\"States\").Medium(),\r\n                        Label(\"Empty\").SetContent(ProgressIndicator().Progress(0).Width(400.px())).AlignCenter(),\r\n                        Label(\"30%\").SetContent(ProgressIndicator().Progress(30).Width(400.px())).AlignCenter(),\r\n                        Label(\"60%\").SetContent(ProgressIndicator().Progress(60).Width(400.px())).AlignCenter(),\r\n                        Label(\"Full\").SetContent(ProgressIndicator().Progress(100).Width(400.px())).AlignCenter(),\r\n                        Label(\"Indeterminate\").SetContent(ProgressIndicator().Indeterminated().Width(400.px())).AlignCenter()\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "ProgressModalSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing System.Threading;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Progress\", Order = 20, Icon = UIcons.WindowMaximize)]\r\n    public class ProgressModalSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ProgressModalSample()\r\n        {\r\n            ProgressModal modal;\r\n\r\n            CancellationTokenSource cts;\r\n\r\n            float progress = 0;\r\n\r\n            void ProgressFrame(object a)\r\n            {\r\n                if (cts.IsCancellationRequested)\r\n                {\r\n                    modal.ProgressSpin().Message(\"Cancelling...\");\r\n                    Task.Delay(2000).ContinueWith(_ => modal.Hide()).FireAndForget();\r\n                    return;\r\n                }\r\n                progress++;\r\n\r\n                if (progress < 100)\r\n                {\r\n                    modal.Message($\"Processing {progress}%\").Progress(progress);\r\n                    window.setTimeout(ProgressFrame, 16);\r\n                }\r\n                else\r\n                {\r\n                    modal.Message(\"Finishing...\").ProgressIndeterminated();\r\n                    Task.Delay(5000).ContinueWith(_ => modal.Hide()).FireAndForget();\r\n                }\r\n            }\r\n\r\n            async Task PlayModal()\r\n            {\r\n                modal = ProgressModal().Title(\"Lorem Ipsum\");\r\n                cts   = new CancellationTokenSource();\r\n\r\n                modal.WithCancel((b) =>\r\n                {\r\n                    b.Disabled();\r\n                    cts.Cancel();\r\n                });\r\n                progress = 0;\r\n                modal.Message(\"Preparing to process...\").ProgressSpin().Show();\r\n                await Task.Delay(1500);\r\n                window.setTimeout(ProgressFrame, 16);\r\n            }\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ProgressModalSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ProgressModal is a specialized modal overlay that combines a title, a message, and a progress indicator. It is used for long-running operations where it is important to block other user interactions until the task is complete, while keeping the user informed of the progress.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use ProgressModal only for operations that truly require the user's focus and shouldn't be interrupted. Ensure that the message provides clear context for what is being processed. Always provide a way to cancel the operation if possible. For background tasks that don't need to block the entire UI, consider using an in-place ProgressIndicator or Spinner instead.\")))\r\n               .Section(\r\n                    Stack().Width(400.px()).Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        Button(\"Open Modal\").OnClick((s, e) => PlayModal().FireAndForget())\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "SpinnerSample": 
                            return "using System;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Progress\", Order = 10, Icon = UIcons.Spinner)]\r\n    public class SpinnerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SpinnerSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(SpinnerSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Spinners are animated circular indicators used to show that a task is in progress when the exact duration is unknown. They are subtle, lightweight, and can be easily placed inline with content or centered within a container to provide feedback without disrupting the layout.\")))\r\n               .Section(Stack().WidthStretch().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use a Spinner for tasks that take more than a second but have an indeterminate end time. Include a brief, descriptive label (e.g., 'Loading...', 'Processing...') to give users context. Choose a size that is appropriate for the surrounding content\u2014smaller for inline elements and larger for full-page loading states. Avoid showing multiple spinners simultaneously if possible.\")))\r\n               .Section(\r\n                    Stack().Width(400.px()).Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        TextBlock(\"Spinner sizes\").Medium(),\r\n                        Label(\"Extra small spinner\").SetContent(Spinner().XSmall()).AlignCenter(),\r\n                        Label(\"Small spinner\").SetContent(Spinner().Small()).AlignCenter(),\r\n                        Label(\"Medium spinner\").SetContent(Spinner().Medium()).AlignCenter(),\r\n                        Label(\"Large spinner\").SetContent(Spinner().Large()).AlignCenter()\r\n                    ))\r\n               .Section(\r\n                    Stack().Width(400.px()).Children(\r\n                        TextBlock(\"Spinner label positioning\").Medium(),\r\n                        Label(\"Spinner with label positioned below\").SetContent(Spinner(\"I am definitely loading...\").Below()),\r\n                        Label(\"Spinner with label positioned above\").SetContent(Spinner(\"Seriously, still loading...\").Above()),\r\n                        Label(\"Spinner with label positioned to right\").SetContent(Spinner(\"Wait, wait...\").Right()),\r\n                        Label(\"Spinner with label positioned to left\").SetContent(Spinner(\"Nope, still loading...\").Left())\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "ContextMenuSample": 
                            return "using static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 20, Icon = UIcons.AppsAdd)]\r\n    public class ContextMenuSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ContextMenuSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ContextMenuSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ContextMenu is a flyout component that displays a list of commands triggered by user interaction, such as a right-click or a button press.\"),\r\n                    TextBlock(\"It provides a focused set of actions relevant to the current context, helping to keep the main interface clean and uncluttered. It supports nested submenus, dividers, headers, and custom component items.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use ContextMenus to surface secondary actions that are relevant to a specific element. Group related commands using dividers. Use submenus sparingly to avoid deep nesting that can be hard to navigate. Ensure that the menu remains within the viewport when opened. Always provide clear labels and icons for common actions.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Simple Context Menu\"),\r\n                    Button(\"Click for Menu\").Var(out var btn1).OnClick((s, e) =>\r\n                        ContextMenu().Items(\r\n                            ContextMenuItem(HStack().Children(Icon(UIcons.Plus), TextBlock(\"New Item\").ML(8))).OnClick((_, __) => Toast().Success(\"New Item created\")),\r\n                            ContextMenuItem(HStack().Children(Icon(UIcons.FolderOpen), TextBlock(\"Open\").ML(8))).OnClick((_, __) => Toast().Information(\"Opening...\")),\r\n                            ContextMenuItem().Divider(),\r\n                            ContextMenuItem(HStack().Children(Icon(UIcons.Trash, color: Theme.Danger.Background), TextBlock(\"Delete\").ML(8).Danger())).OnClick((_, __) => Toast().Error(\"Deleted\"))\r\n                        ).ShowFor(btn1)\r\n                    ).MB(16),\r\n                    SampleSubTitle(\"Menu with Submenus and Headers\"),\r\n                    Button(\"Complex Menu\").Var(out var btn2).OnClick((s, e) =>\r\n                        ContextMenu().Items(\r\n                            ContextMenuItem(\"Actions\").Header(),\r\n                            ContextMenuItem(HStack().Children(Icon(UIcons.Edit), TextBlock(\"Edit\").ML(8))).SubMenu(\r\n                                ContextMenu().Items(\r\n                                    ContextMenuItem(\"Edit Name\"),\r\n                                    ContextMenuItem(\"Edit Permissions\"),\r\n                                    ContextMenuItem(\"Edit Metadata\")\r\n                                )\r\n                            ),\r\n                            ContextMenuItem(HStack().Children(Icon(UIcons.Share), TextBlock(\"Share\").ML(8))).SubMenu(\r\n                                ContextMenu().Items(\r\n                                    ContextMenuItem(\"Copy Link\"),\r\n                                    ContextMenuItem(\"Email Link\")\r\n                                )\r\n                            ),\r\n                            ContextMenuItem().Divider(),\r\n                            ContextMenuItem(\"Advanced\").Header(),\r\n                            ContextMenuItem(\"Properties\").Disabled(),\r\n                            ContextMenuItem(HStack().Children(Icon(UIcons.Settings), TextBlock(\"Settings\").ML(8)))\r\n                        ).ShowFor(btn2)\r\n                    )\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "DialogSample": 
                            return "using System;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 10, Icon = UIcons.WindowMinimize)]\r\n    public class DialogSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public DialogSample()\r\n        {\r\n            var dialog   = Dialog(\"Sample Dialog\");\r\n            var response = TextBlock();\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(DialogSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Dialogs are modal UI overlays that provide contextual information or require user action, such as confirmation or input. They are designed to capture the user's attention and typically block interaction with the rest of the application until they are dismissed.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Dialogs for critical or short-term tasks that require a decision. Ensure the content is brief and clearly states the purpose. Provide logical action buttons (e.g., 'Confirm' and 'Cancel') and highlight the primary action. Avoid overusing Dialogs for non-essential information to prevent frustrating the user. Consider using non-modal alternatives if the task doesn't require immediate attention.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    Button(\"Open Dialog\").OnClick((c, ev) => dialog.Show()),\r\n                    HStack().Children(\r\n                        Button(\"Open YesNo\").OnClick((c,       ev) => Dialog(\"Sample Dialog\").YesNo(() => response.Text(\"Clicked Yes\"), () => response.Text(\"Clicked No\"))),\r\n                        Button(\"Open YesNoCancel\").OnClick((c, ev) => Dialog(\"Sample Dialog\").YesNoCancel(() => response.Text(\"Clicked Yes\"), () => response.Text(\"Clicked No\"), () => response.Text(\"Clicked Cancel\"))),\r\n                        Button(\"Open Ok\").OnClick((c,          ev) => Dialog(\"Sample Dialog\").Ok(() => response.Text(\"Clicked Ok\"))),\r\n                        Button(\"Open RetryCancel\").OnClick((c, ev) => Dialog(\"Sample Dialog\").RetryCancel(() => response.Text(\"Clicked Retry\"), () => response.Text(\"Clicked Cancel\")))),\r\n                    Button(\"Open YesNo with dark overlay\").OnClick((c,       ev) => Dialog(\"Sample Dialog\").Dark().YesNo(() => response.Text(\"Clicked Yes\"), () => response.Text(\"Clicked No\"), y => y.Success().SetText(\"Yes!\"), n => n.Danger().SetText(\"Nope\"))),\r\n                    Button(\"Open YesNoCancel with dark overlay\").OnClick((c, ev) => Dialog(\"Sample Dialog\").Dark().YesNoCancel(() => response.Text(\"Clicked Yes\"), () => response.Text(\"Clicked No\"), () => response.Text(\"Clicked Cancel\"))),\r\n                    Button(\"Open Ok with dark overlay\").OnClick((c,          ev) => Dialog(\"Sample Dialog\").Dark().Ok(() => response.Text(\"Clicked Ok\"))),\r\n                    Button(\"Open RetryCancel with dark overlay\").OnClick((c, ev) => Dialog(\"Sample Dialog\").Dark().RetryCancel(() => response.Text(\"Clicked Retry\"), () => response.Text(\"Clicked Cancel\"))),\r\n                    response));\r\n\r\n            dialog.Content(Stack().Children(TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit.\"),\r\n                    Toggle(\"Is draggable\").OnChange((c,    ev) => dialog.IsDraggable = c.IsChecked),\r\n                    Toggle(\"Is dark overlay\").OnChange((c, ev) => dialog.IsDark      = c.IsChecked).Checked(dialog.IsDark)\r\n                ))\r\n               .Commands(Button(\"Send\").Primary().AlignEnd().OnClick((c, ev) => dialog.Hide()), Button(\"Don`t send\").AlignEnd().OnClick((c, ev) => dialog.Hide()));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "FloatSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 0, Icon = UIcons.GameBoardAlt)]\r\n    public class FloatSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public FloatSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(FloatSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Float components are used to place content in absolute-positioned overlays within a relative container. They allow for precise placement of UI elements, such as badges, help icons, or status indicators, without affecting the layout of surrounding components.\")))\r\n               .Section(Stack().WidthStretch().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Float when you need to position an element independently of the normal document flow. Always ensure the parent container is set to 'Relative' positioning to constrain the floated element. Be careful not to obscure important content or interactive elements beneath the overlay. Use meaningful positions that correlate logically with the parent content.\")))\r\n               .Section(\r\n                    Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        TextBlock(\"Possible Positions\").Medium(),\r\n                        Stack().Relative().Horizontal().WS().Height(400.px()).Children(\r\n                            Float(Button(\"TopLeft\"),      Float.Position.TopLeft),\r\n                            Float(Button(\"TopMiddle\"),    Float.Position.TopMiddle),\r\n                            Float(Button(\"TopRight\"),     Float.Position.TopRight),\r\n                            Float(Button(\"LeftCenter\"),   Float.Position.LeftCenter),\r\n                            Float(Button(\"Center\"),       Float.Position.Center),\r\n                            Float(Button(\"RightCenter\"),  Float.Position.RightCenter),\r\n                            Float(Button(\"BottomLeft\"),   Float.Position.BottomLeft),\r\n                            Float(Button(\"BottonMiddle\"), Float.Position.BottonMiddle),\r\n                            Float(Button(\"BottomRight\"),  Float.Position.BottomRight)\r\n                        )));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "LayerSample": 
                            return "using Tesserae;\r\nusing Tesserae.Tests;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 0, Icon = UIcons.TablePivot)]\r\n    public class LayerSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        public LayerSample()\r\n        {\r\n            var layer     = Layer();\r\n            var layer2    = Layer();\r\n            var layerHost = LayerHost();\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(LayerSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Layer is a technical component used to render content outside of its parent's DOM tree, typically at the end of the document body. This allows content to escape boundaries like 'overflow: hidden' or complex z-index stacks, ensuring that elements like tooltips, context menus, and modals always appear on top of other content.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Layers for UI elements that must appear above all other content regardless of their position in the component hierarchy. Utilize 'LayerHost' when you need to project layered content into a specific part of the DOM instead of the default body location. Be mindful of the lifecycle of layered components to ensure they are properly removed from the DOM when no longer needed.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    TextBlock(\"Basic layered content\").Medium(),\r\n                    layer.Content(HStack().Children(TextBlock(\"This is example layer content.\"),\r\n                        Button(\"Show second Layer\").SetIcon(UIcons.Add).Primary().OnClick((s, e) => layer2.IsVisible = true),\r\n                        layer2.Content(HStack().Children(TextBlock(\"This is the second example layer content.\"),\r\n                            Button(\"Hide second Layer\").SetIcon(UIcons.CrossCircle).Primary().OnClick((s, e) => layer2.IsVisible = false)\r\n                        )),\r\n                        Toggle(\"Toggle Component Layer\").OnChange((s, e) => layer.IsVisible = s.IsChecked), Toggle())),\r\n                    Toggle(\"Toggle Component Layer\").OnChange((s, e) => layer.IsVisible = s.IsChecked),\r\n                    TextBlock(\"Using LayerHost to control projection\").Medium(),\r\n                    Toggle(\"Show on Host\").OnChange((s, e) => layer.Host = s.IsChecked ? layerHost : null),\r\n                    layerHost));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}";
                        case "ModalSample": 
                            return "using Tesserae;\r\nusing Tesserae.Tests;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 10, Icon = UIcons.WindowRestore)]\r\n    public class ModalSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ModalSample()\r\n        {\r\n            var container = Raw();\r\n\r\n            Modal(\"Sample Modal\")\r\n               .Var(out var modal)\r\n               .LightDismiss()\r\n               .Width(60.vw())\r\n               .Height(60.vh())\r\n               .SetFooter(TextBlock(\"This is a footer note\").SemiBold())\r\n               .Content(Stack().Children(\r\n                    TextBlock(\"Modals provide a focused environment for users to complete a task or view important information. They can be configured with various options like dark overlays, non-blocking behavior, and draggable headers.\"),\r\n                    Label(\"Light Dismiss\").Inline().AutoWidth().SetContent(Toggle().OnChange((s,            e) => modal.CanLightDismiss     = s.IsChecked).Checked(modal.CanLightDismiss)),\r\n                    Label(\"Is draggable\").Inline().AutoWidth().SetContent(Toggle().OnChange((s,             e) => modal.IsDraggable         = s.IsChecked).Checked(modal.IsDraggable)),\r\n                    Label(\"Is dark overlay\").Inline().AutoWidth().SetContent(Toggle().OnChange((s,          e) => modal.IsDark              = s.IsChecked).Checked(modal.IsDark)),\r\n                    Label(\"Is non-blocking\").Inline().AutoWidth().SetContent(Toggle().OnChange((s,          e) => modal.IsNonBlocking       = s.IsChecked).Checked(modal.IsNonBlocking)),\r\n                    Label(\"Hide close button\").Inline().AutoWidth().SetContent(Toggle().OnChange((s,        e) => modal.WillShowCloseButton = !s.IsChecked).Checked(!modal.WillShowCloseButton)),\r\n                    Label(\"Open a dialog from here\").Var(out var lbl).SetContent(Button(\"Open\").OnClick((s, e) => Dialog(\"Dialog over Modal\").Content(TextBlock(\"Hello World!\")).YesNo(() => lbl.Text = \"Yes\", () => lbl.Text = \"No\")))));\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ModalSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Modals are large overlays used for tasks that require a separate context, such as creating or editing complex entities, or for displaying rich content that shouldn't clutter the main interface. They provide more space than Dialogs and can host a variety of components.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Modals for multi-step tasks or content-heavy interactions. Ensure that the Modal has a clear title and provide multiple ways to dismiss it (e.g., Close button, clicking outside, or the Escape key). Use 'LightDismiss' for non-critical information and blocking behavior only when user input is essential. Always maintain a clear typographic hierarchy within the Modal content.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    Button(\"Open Modal\").OnClick((s,                   e) => modal.Show()),\r\n                    Button(\"Open Modal from top right\").OnClick((s,    e) => modal.ShowAt(fromRight: 16.px(), fromTop: 16.px())),\r\n                    Button(\"Open Modal with minimum size\").OnClick((s, e) => Modal().CenterContent().LightDismiss().Dark().Content(TextBlock(\"small content\").Tiny()).MinHeight(50.vh()).MinWidth(50.vw()).Show()),\r\n                    SampleTitle(\"Embedded Modal\"),\r\n                    Button(\"Open Modal Below\").OnClick((s, e) => container.Content(Modal(\"Embedded Modal\").CenterContent().LightDismiss().Dark().Content(TextBlock(\"hosted small content\").Tiny()).MinHeight(30.vh()).MinWidth(50.vw()).ShowEmbedded())),\r\n                    container\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "PanelSample": 
                            return "using Tesserae;\r\nusing Tesserae.Tests;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\nusing Panel = Tesserae.Panel;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 10, Icon = UIcons.WindowMinimize)]\r\n    public class PanelSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public PanelSample()\r\n        {\r\n            var panel = Panel().LightDismiss();\r\n\r\n            panel.Content(\r\n                Stack().Children(\r\n                    TextBlock(\"Sample panel\").MediumPlus().SemiBold(),\r\n                    ChoiceGroup(\"Side:\").Choices(\r\n                        Choice(\"Far\").Selected().OnSelected(x => panel.Side = Panel.PanelSide.Far),\r\n                        Choice(\"Near\").OnSelected(x => panel.Side           = Panel.PanelSide.Near)\r\n                    ),\r\n                    Toggle(\"Light Dismiss\").OnChange((s, e) => panel.CanLightDismiss = s.IsChecked).Checked(panel.CanLightDismiss),\r\n                    ChoiceGroup(\"Size:\").Choices(\r\n                        Choice(\"Small\").Selected().OnSelected(x => panel.Size = Panel.PanelSize.Small),\r\n                        Choice(\"Medium\").OnSelected(x => panel.Size           = Panel.PanelSize.Medium),\r\n                        Choice(\"Large\").OnSelected(x => panel.Size            = Panel.PanelSize.Large),\r\n                        Choice(\"LargeFixed\").OnSelected(x => panel.Size       = Panel.PanelSize.LargeFixed),\r\n                        Choice(\"ExtraLarge\").OnSelected(x => panel.Size       = Panel.PanelSize.ExtraLarge),\r\n                        Choice(\"FullWidth\").OnSelected(x => panel.Size        = Panel.PanelSize.FullWidth)\r\n                    ),\r\n                    Toggle(\"Is non-blocking\").OnChange((s,   e) => panel.IsNonBlocking   = s.IsChecked).Checked(panel.IsNonBlocking),\r\n                    Toggle(\"Is dark overlay\").OnChange((s,   e) => panel.IsDark          = s.IsChecked).Checked(panel.IsDark),\r\n                    Toggle(\"Hide close button\").OnChange((s, e) => panel.ShowCloseButton = !s.IsChecked).Checked(!panel.ShowCloseButton)\r\n                )).SetFooter(HStack().Children(Button(\"Footer Button 1\").Primary(), Button(\"Footer Button 2\")));\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(PanelSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Panels are sliding overlays typically used for creation or management tasks, such as editing a user's profile or configuring settings. They provide a large, temporary surface that slides in from either the left or right side of the screen, keeping the user within the current context while providing space for complex forms or information.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Panels for self-contained tasks that are too large for a Dialog or Modal. Choose the 'Far' side (right) for most common actions, and 'Near' (left) for navigation-related content. Provide clear 'Save' and 'Cancel' actions in the footer. Ensure that the Panel size is appropriate for its content, using wider variants for complex forms. Use 'LightDismiss' to allow users to quickly exit by clicking outside the panel.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    Button(\"Open panel\").OnClick((s, e) => panel.Show())));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "PivotSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 20, Icon = UIcons.TableLayout)]\r\n    public class PivotSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent content;\r\n\r\n        public PivotSample()\r\n        {\r\n            content = SectionStack()\r\n               .Title(SampleHeader(nameof(PivotSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Pivots are used for navigating between different views or categories of content within the same context. They provide a compact way to switch between related data sets, such as different tabs in a settings page or different views of a list.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Pivots to organize content into logical categories. Keep labels short and descriptive. Ensure that the most frequently used views are placed first. Utilize the 'Justified' or 'Centered' styles when the pivot should span the full width of its container. Use the 'Cached' option to preserve the state of a tab's content when switching away.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Normal Style\"),\r\n                    GetPivot(),\r\n                    SampleSubTitle(\"Justified Style\"),\r\n                    GetPivot().Justified(),\r\n                    SampleSubTitle(\"Centered Style\"),\r\n                    GetPivot().Centered(),\r\n                    SampleSubTitle(\"Cached vs. Not Cached Tabs\"),\r\n                    Pivot().Pivot(\"tab1\", PivotTitle(\"Cached\"),     () => TextBlock(DateTimeOffset.UtcNow.ToString()).Regular(), cached: true)\r\n                           .Pivot(\"tab2\", PivotTitle(\"Not Cached\"), () => TextBlock(DateTimeOffset.UtcNow.ToString()).Regular(), cached: false),\r\n                    SampleSubTitle(\"Cached vs. Not Cached Tabs\"),\r\n                    SampleSubTitle(\"Scroll with limited height\"),\r\n                    Pivot().MaxHeight(500.px())\r\n                       .Pivot(\"tab1\", PivotTitle(\"5 Items\"),   () => ItemsList(GetSomeItems(5)).PB(16),   cached: true)\r\n                       .Pivot(\"tab2\", PivotTitle(\"10 Items\"),  () => ItemsList(GetSomeItems(20)).PB(16),  cached: true)\r\n                       .Pivot(\"tab3\", PivotTitle(\"50 Items\"),  () => ItemsList(GetSomeItems(50)).PB(16),  cached: true)\r\n                       .Pivot(\"tab4\", PivotTitle(\"100 Items\"), () => ItemsList(GetSomeItems(100)).PB(16), cached: true),\r\n                    SampleSubTitle(\"Tab Overflow\"),\r\n                    SplitView().Resizable().WS().H(500).LeftIsSmaller(300.px()).Left(\r\n                    Pivot().S()\r\n                       .Pivot(\"tab1\", PivotTitle(\"Tab 1\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)\r\n                       .Pivot(\"tab2\", PivotTitle(\"Tab 2\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)\r\n                       .Pivot(\"tab3\", PivotTitle(\"Tab 3\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)\r\n                       .Pivot(\"tab4\", PivotTitle(\"Tab 4\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)\r\n                       .Pivot(\"tab5\", PivotTitle(\"Tab 5\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)\r\n                       .Pivot(\"tab6\", PivotTitle(\"Tab 6\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)\r\n                       .Pivot(\"tab7\", PivotTitle(\"Tab 7\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)\r\n                       .Pivot(\"tab8\", PivotTitle(\"Tab 8\"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true))\r\n                    .Right(TextBlock(\"\ud83d\udc48 resize this area to see the overflow adjusting which tabs to show\").WS().BreakSpaces())\r\n                ));\r\n        }\r\n\r\n        private Pivot GetPivot()\r\n        {\r\n            return Pivot().Pivot(\"first-tab\",  PivotTitle(\"First Tab\"),  () => TextBlock(\"First Tab\"))\r\n                          .Pivot(\"second-tab\", PivotTitle(\"Second Tab\"), () => TextBlock(\"Second Tab\"))\r\n                          .Pivot(\"third-tab\",  PivotTitle(\"Third Tab\"),  () => TextBlock(\"Third Tab\"));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return content.Render();\r\n        }\r\n\r\n        private IComponent[] GetSomeItems(int count)\r\n        {\r\n            return Enumerable\r\n               .Range(1, count)\r\n               .Select(number => Card(TextBlock($\"Lorem Ipsum {number}\").NonSelectable()).MinWidth(200.px()))\r\n               .ToArray();\r\n        }\r\n    }\r\n}";
                        case "PivotSelectorSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 20, Icon = UIcons.TableLayout)]\r\n    public class PivotSelectorSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent content;\r\n\r\n        public PivotSelectorSample()\r\n        {\r\n            content = SectionStack()\r\n               .Title(SampleHeader(nameof(PivotSelectorSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"PivotSelector is a variation of the Pivot component that uses a Dropdown for navigation. It is particularly effective for mobile-first designs or interfaces with a large number of tabs that would otherwise require excessive horizontal scrolling.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use PivotSelector when horizontal space is constrained or when the number of tabs is dynamic and potentially large. Provide clear icons and text for each tab to aid navigation. Utilize the 'SetCommands' feature to surface global actions relevant to all tabs, such as 'Add New' or 'Refresh'.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"Basic PivotSelector\"),\r\n                    PivotSelector()\r\n                        .Pivot(\"tab1\", \"Tab 1\", () => Card(TextBlock(\"Content for Tab 1\").P(32)))\r\n                        .Pivot(\"tab2\", \"Tab 2\", () => Card(TextBlock(\"Content for Tab 2\").P(32)))\r\n                        .Pivot(\"tab3\", \"Tab 3\", () => Card(TextBlock(\"Content for Tab 3\").P(32))),\r\n                    SampleSubTitle(\"PivotSelector with custom buttons\").PT(16),\r\n                    PivotSelector()\r\n                        .SetCommands(\r\n                            Button().SetIcon(UIcons.Add).NoBorder().NoBackground().OnClick(() => alert(\"Add clicked\")),\r\n                            Button().SetIcon(UIcons.Settings).NoBorder().NoBackground().OnClick(() => alert(\"Settings clicked\"))\r\n                        )\r\n                        .Pivot(\"tab1\", () => Button(\"Tab 1\").NoBackground().NoBorder().SetIcon(UIcons.Rocket), () => Card(TextBlock(\"Content for Tab 1\").P(32)))\r\n                        .Pivot(\"tab2\", () => Button(\"Tab 2\").NoBackground().NoBorder().SetIcon(UIcons.Car),    () => Card(TextBlock(\"Content for Tab 2\").P(32))),\r\n                    SampleSubTitle(\"PivotSelector with large number of tabs\").PT(16),\r\n                    PivotSelector()\r\n                        .Pivot(Enumerable.Range(1, 20).Select(i => ($\"tab{i}\", $\"Tab {i}\", (Func<IComponent>)(() => Card(TextBlock($\"Content for Tab {i}\").P(32))))).ToArray())\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return content.Render();\r\n        }\r\n    }\r\n}\r\n";
                        case "SectionStackSample": 
                            return "using System;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 20, Icon = UIcons.BorderAll)]\r\n    public class SectionStackSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public SectionStackSample()\r\n        {\r\n            var stack = SectionStack();\r\n\r\n            _content = Stack().Children(SectionStack().Title(SampleHeader(nameof(SectionStackSample)))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Overview\"),\r\n                        TextBlock(\"SectionStack is a high-level layout component designed for creating long-form pages or detailed views. It organizes content into distinct vertical sections, typically with a header and footer, providing a consistent structure for complex information architectures.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Best Practices\"),\r\n                        TextBlock(\"Use SectionStack for the main content area of your pages. Organize related components into distinct sections to improve readability and scanability. Utilize the 'Title' and 'Commands' features of the SectionStack to provide context and actions at the top of the page.\")))\r\n                   .Section(Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        SampleSubTitle(\"Dynamic Section Generation\"),\r\n                        Label(\"Number of sections:\").SetContent(Slider(5, 0, 10, 1).OnInput((s, e) => SetChildren(stack, s.Value))))),\r\n                stack);\r\n            SetChildren(stack, 5);\r\n        }\r\n\r\n        private void SetChildren(SectionStack stack, int count)\r\n        {\r\n            stack.Clear();\r\n\r\n            for (int i = 0; i < count; i++)\r\n            {\r\n                stack.Section(Stack().Children(\r\n                    TextBlock($\"Section {i}\").MediumPlus().SemiBold(),\r\n                    TextBlock(\"Wrap (Default)\").SmallPlus(),\r\n                    TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\").Width(50.percent()),\r\n                    TextBlock(\"No Wrap\").SmallPlus(),\r\n                    TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\").NoWrap().Width(50.percent())\r\n                ));\r\n            }\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "TutorialModalSample": 
                            return "using Tesserae;\r\nusing Tesserae.Tests;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Surfaces\", Order = 20, Icon = UIcons.Indent)]\r\n    public class TutorialModalSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public TutorialModalSample()\r\n        {\r\n            var container = Raw();\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(TutorialModalSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"TutorialModal is a specialized modal designed for guided processes, such as onboarding or feature walkthroughs. It combines a large content area with a dedicated help panel and an optional illustrative image, providing a structured environment for users to learn while they interact.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use TutorialModals for complex tasks that benefit from additional explanation and guidance. Ensure that the help text is clear and directly relates to the fields in the content area. Use images or icons to provide visual cues. Always provide a clear way for users to complete or discard the process. Avoid overwhelming users with too much information; keep both the content and the help text concise.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    Button(\"Open Tutorial Modal\").OnClick((s,       e) => SampleTutorialModal().Show()),\r\n                    Button(\"Open Large Tutorial Modal\").OnClick((s, e) => SampleTutorialModal().Height(90.vh()).Width(90.vw()).Show()),\r\n                    SampleTitle(\"Embedded Modal\"),\r\n                    Button(\"Open Modal Below\").OnClick((s, e) => container.Content(SampleTutorialModal().Border(\"#ffaf66\", 5.px()).ShowEmbedded())),\r\n                    container\r\n                ));\r\n        }\r\n\r\n        private static TutorialModal SampleTutorialModal()\r\n        {\r\n            return TutorialModal()\r\n               .Var(out var tutorialModal)\r\n               .SetTitle(\"This is a Tutorial Modal\")\r\n               .SetHelpText(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit,<b> sed do </b> eiusmod tempor incididunt ut labore et dolore magna aliqua. \", treatAsHTML: true)\r\n               .SetImageSrc(\"./assets/img/box-img.svg\", 16.px())\r\n               .SetContent(\r\n                    VStack().S().ScrollY().Children(\r\n                        Label(\"Input 1\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 2\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 3\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 4\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 5\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 6\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 7\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 8\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\")),\r\n                        Label(\"Input 9\").SetContent(TextBox().SetPlaceholder(\"Enter your input here...\"))))\r\n               .SetFooterCommands(\r\n                    Button(\"Discard\").OnClick((_,        __) => tutorialModal.Hide()),\r\n                    Button(\"Save\").Primary().OnClick((_, __) => tutorialModal.Hide()));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "ColorsSample": 
                            return "using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 10, Icon = UIcons.Palette)]\r\n    public class ColorsSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ColorsSample()\r\n        {\r\n            var allColors = new (string Name, string Value)[]\r\n            {\r\n                (nameof(Theme.Colors.Lime100), Theme.Colors.Lime100),\r\n                (nameof(Theme.Colors.Lime200), Theme.Colors.Lime200),\r\n                (nameof(Theme.Colors.Lime250), Theme.Colors.Lime250),\r\n                (nameof(Theme.Colors.Lime300), Theme.Colors.Lime300),\r\n                (nameof(Theme.Colors.Lime400), Theme.Colors.Lime400),\r\n                (nameof(Theme.Colors.Lime500), Theme.Colors.Lime500),\r\n                (nameof(Theme.Colors.Lime600), Theme.Colors.Lime600),\r\n                (nameof(Theme.Colors.Lime700), Theme.Colors.Lime700),\r\n                (nameof(Theme.Colors.Lime800), Theme.Colors.Lime800),\r\n                (nameof(Theme.Colors.Lime850), Theme.Colors.Lime850),\r\n                (nameof(Theme.Colors.Lime900), Theme.Colors.Lime900),\r\n                (nameof(Theme.Colors.Lime1000), Theme.Colors.Lime1000),\r\n                (nameof(Theme.Colors.Red100), Theme.Colors.Red100),\r\n                (nameof(Theme.Colors.Red200), Theme.Colors.Red200),\r\n                (nameof(Theme.Colors.Red250), Theme.Colors.Red250),\r\n                (nameof(Theme.Colors.Red300), Theme.Colors.Red300),\r\n                (nameof(Theme.Colors.Red400), Theme.Colors.Red400),\r\n                (nameof(Theme.Colors.Red500), Theme.Colors.Red500),\r\n                (nameof(Theme.Colors.Red600), Theme.Colors.Red600),\r\n                (nameof(Theme.Colors.Red700), Theme.Colors.Red700),\r\n                (nameof(Theme.Colors.Red800), Theme.Colors.Red800),\r\n                (nameof(Theme.Colors.Red850), Theme.Colors.Red850),\r\n                (nameof(Theme.Colors.Red900), Theme.Colors.Red900),\r\n                (nameof(Theme.Colors.Red1000), Theme.Colors.Red1000),\r\n                (nameof(Theme.Colors.Orange100), Theme.Colors.Orange100),\r\n                (nameof(Theme.Colors.Orange200), Theme.Colors.Orange200),\r\n                (nameof(Theme.Colors.Orange250), Theme.Colors.Orange250),\r\n                (nameof(Theme.Colors.Orange300), Theme.Colors.Orange300),\r\n                (nameof(Theme.Colors.Orange400), Theme.Colors.Orange400),\r\n                (nameof(Theme.Colors.Orange500), Theme.Colors.Orange500),\r\n                (nameof(Theme.Colors.Orange600), Theme.Colors.Orange600),\r\n                (nameof(Theme.Colors.Orange700), Theme.Colors.Orange700),\r\n                (nameof(Theme.Colors.Orange800), Theme.Colors.Orange800),\r\n                (nameof(Theme.Colors.Orange850), Theme.Colors.Orange850),\r\n                (nameof(Theme.Colors.Orange900), Theme.Colors.Orange900),\r\n                (nameof(Theme.Colors.Orange1000), Theme.Colors.Orange1000),\r\n                (nameof(Theme.Colors.Yellow100), Theme.Colors.Yellow100),\r\n                (nameof(Theme.Colors.Yellow200), Theme.Colors.Yellow200),\r\n                (nameof(Theme.Colors.Yellow250), Theme.Colors.Yellow250),\r\n                (nameof(Theme.Colors.Yellow300), Theme.Colors.Yellow300),\r\n                (nameof(Theme.Colors.Yellow400), Theme.Colors.Yellow400),\r\n                (nameof(Theme.Colors.Yellow500), Theme.Colors.Yellow500),\r\n                (nameof(Theme.Colors.Yellow600), Theme.Colors.Yellow600),\r\n                (nameof(Theme.Colors.Yellow700), Theme.Colors.Yellow700),\r\n                (nameof(Theme.Colors.Yellow800), Theme.Colors.Yellow800),\r\n                (nameof(Theme.Colors.Yellow850), Theme.Colors.Yellow850),\r\n                (nameof(Theme.Colors.Yellow900), Theme.Colors.Yellow900),\r\n                (nameof(Theme.Colors.Yellow1000), Theme.Colors.Yellow1000),\r\n                (nameof(Theme.Colors.Green100), Theme.Colors.Green100),\r\n                (nameof(Theme.Colors.Green200), Theme.Colors.Green200),\r\n                (nameof(Theme.Colors.Green250), Theme.Colors.Green250),\r\n                (nameof(Theme.Colors.Green300), Theme.Colors.Green300),\r\n                (nameof(Theme.Colors.Green400), Theme.Colors.Green400),\r\n                (nameof(Theme.Colors.Green500), Theme.Colors.Green500),\r\n                (nameof(Theme.Colors.Green600), Theme.Colors.Green600),\r\n                (nameof(Theme.Colors.Green700), Theme.Colors.Green700),\r\n                (nameof(Theme.Colors.Green800), Theme.Colors.Green800),\r\n                (nameof(Theme.Colors.Green850), Theme.Colors.Green850),\r\n                (nameof(Theme.Colors.Green900), Theme.Colors.Green900),\r\n                (nameof(Theme.Colors.Green1000), Theme.Colors.Green1000),\r\n                (nameof(Theme.Colors.Teal100), Theme.Colors.Teal100),\r\n                (nameof(Theme.Colors.Teal200), Theme.Colors.Teal200),\r\n                (nameof(Theme.Colors.Teal250), Theme.Colors.Teal250),\r\n                (nameof(Theme.Colors.Teal300), Theme.Colors.Teal300),\r\n                (nameof(Theme.Colors.Teal400), Theme.Colors.Teal400),\r\n                (nameof(Theme.Colors.Teal500), Theme.Colors.Teal500),\r\n                (nameof(Theme.Colors.Teal600), Theme.Colors.Teal600),\r\n                (nameof(Theme.Colors.Teal700), Theme.Colors.Teal700),\r\n                (nameof(Theme.Colors.Teal800), Theme.Colors.Teal800),\r\n                (nameof(Theme.Colors.Teal850), Theme.Colors.Teal850),\r\n                (nameof(Theme.Colors.Teal900), Theme.Colors.Teal900),\r\n                (nameof(Theme.Colors.Teal1000), Theme.Colors.Teal1000),\r\n                (nameof(Theme.Colors.Blue100), Theme.Colors.Blue100),\r\n                (nameof(Theme.Colors.Blue200), Theme.Colors.Blue200),\r\n                (nameof(Theme.Colors.Blue250), Theme.Colors.Blue250),\r\n                (nameof(Theme.Colors.Blue300), Theme.Colors.Blue300),\r\n                (nameof(Theme.Colors.Blue400), Theme.Colors.Blue400),\r\n                (nameof(Theme.Colors.Blue500), Theme.Colors.Blue500),\r\n                (nameof(Theme.Colors.Blue600), Theme.Colors.Blue600),\r\n                (nameof(Theme.Colors.Blue700), Theme.Colors.Blue700),\r\n                (nameof(Theme.Colors.Blue800), Theme.Colors.Blue800),\r\n                (nameof(Theme.Colors.Blue850), Theme.Colors.Blue850),\r\n                (nameof(Theme.Colors.Blue900), Theme.Colors.Blue900),\r\n                (nameof(Theme.Colors.Blue1000), Theme.Colors.Blue1000),\r\n                (nameof(Theme.Colors.Purple100), Theme.Colors.Purple100),\r\n                (nameof(Theme.Colors.Purple200), Theme.Colors.Purple200),\r\n                (nameof(Theme.Colors.Purple250), Theme.Colors.Purple250),\r\n                (nameof(Theme.Colors.Purple300), Theme.Colors.Purple300),\r\n                (nameof(Theme.Colors.Purple400), Theme.Colors.Purple400),\r\n                (nameof(Theme.Colors.Purple500), Theme.Colors.Purple500),\r\n                (nameof(Theme.Colors.Purple600), Theme.Colors.Purple600),\r\n                (nameof(Theme.Colors.Purple700), Theme.Colors.Purple700),\r\n                (nameof(Theme.Colors.Purple800), Theme.Colors.Purple800),\r\n                (nameof(Theme.Colors.Purple850), Theme.Colors.Purple850),\r\n                (nameof(Theme.Colors.Purple900), Theme.Colors.Purple900),\r\n                (nameof(Theme.Colors.Purple1000), Theme.Colors.Purple1000),\r\n                (nameof(Theme.Colors.Magenta100), Theme.Colors.Magenta100),\r\n                (nameof(Theme.Colors.Magenta200), Theme.Colors.Magenta200),\r\n                (nameof(Theme.Colors.Magenta250), Theme.Colors.Magenta250),\r\n                (nameof(Theme.Colors.Magenta300), Theme.Colors.Magenta300),\r\n                (nameof(Theme.Colors.Magenta400), Theme.Colors.Magenta400),\r\n                (nameof(Theme.Colors.Magenta500), Theme.Colors.Magenta500),\r\n                (nameof(Theme.Colors.Magenta600), Theme.Colors.Magenta600),\r\n                (nameof(Theme.Colors.Magenta700), Theme.Colors.Magenta700),\r\n                (nameof(Theme.Colors.Magenta800), Theme.Colors.Magenta800),\r\n                (nameof(Theme.Colors.Magenta850), Theme.Colors.Magenta850),\r\n                (nameof(Theme.Colors.Magenta900), Theme.Colors.Magenta900),\r\n                (nameof(Theme.Colors.Magenta1000), Theme.Colors.Magenta1000),\r\n                (nameof(Theme.Colors.Neutral0), Theme.Colors.Neutral0),\r\n                (nameof(Theme.Colors.Neutral100), Theme.Colors.Neutral100),\r\n                (nameof(Theme.Colors.Neutral200), Theme.Colors.Neutral200),\r\n                (nameof(Theme.Colors.Neutral300), Theme.Colors.Neutral300),\r\n                (nameof(Theme.Colors.Neutral400), Theme.Colors.Neutral400),\r\n                (nameof(Theme.Colors.Neutral500), Theme.Colors.Neutral500),\r\n                (nameof(Theme.Colors.Neutral600), Theme.Colors.Neutral600),\r\n                (nameof(Theme.Colors.Neutral700), Theme.Colors.Neutral700),\r\n                (nameof(Theme.Colors.Neutral800), Theme.Colors.Neutral800),\r\n                (nameof(Theme.Colors.Neutral900), Theme.Colors.Neutral900),\r\n                (nameof(Theme.Colors.Neutral1000), Theme.Colors.Neutral1000),\r\n                (nameof(Theme.Colors.Neutral1100), Theme.Colors.Neutral1100),\r\n                (nameof(Theme.Colors.DarkNeutral0), Theme.Colors.DarkNeutral0),\r\n                (nameof(Theme.Colors.DarkNeutral100), Theme.Colors.DarkNeutral100),\r\n                (nameof(Theme.Colors.DarkNeutral200), Theme.Colors.DarkNeutral200),\r\n                (nameof(Theme.Colors.DarkNeutral300), Theme.Colors.DarkNeutral300),\r\n                (nameof(Theme.Colors.DarkNeutral400), Theme.Colors.DarkNeutral400),\r\n                (nameof(Theme.Colors.DarkNeutral500), Theme.Colors.DarkNeutral500),\r\n                (nameof(Theme.Colors.DarkNeutral600), Theme.Colors.DarkNeutral600),\r\n                (nameof(Theme.Colors.DarkNeutral700), Theme.Colors.DarkNeutral700),\r\n                (nameof(Theme.Colors.DarkNeutral800), Theme.Colors.DarkNeutral800),\r\n                (nameof(Theme.Colors.DarkNeutral900), Theme.Colors.DarkNeutral900),\r\n                (nameof(Theme.Colors.DarkNeutral1000), Theme.Colors.DarkNeutral1000),\r\n                (nameof(Theme.Colors.DarkNeutral1100), Theme.Colors.DarkNeutral1100),\r\n            };\r\n\r\n            //TODO: Add alpha colors over an image or background\r\n            //(nameof(Theme.Colors.Neutral100Alpha), Theme.Colors.Neutral100Alpha),\r\n            //(nameof(Theme.Colors.Neutral200Alpha), Theme.Colors.Neutral200Alpha),\r\n            //(nameof(Theme.Colors.Neutral300Alpha), Theme.Colors.Neutral300Alpha),\r\n            //(nameof(Theme.Colors.Neutral400Alpha), Theme.Colors.Neutral400Alpha),\r\n            //(nameof(Theme.Colors.Neutral500Alpha), Theme.Colors.Neutral500Alpha),\r\n\r\n            //(nameof(Theme.Colors.DarkNeutral100Alpha), Theme.Colors.DarkNeutral100Alpha),\r\n            //(nameof(Theme.Colors.DarkNeutral200Alpha), Theme.Colors.DarkNeutral200Alpha),\r\n            //(nameof(Theme.Colors.DarkNeutral300Alpha), Theme.Colors.DarkNeutral300Alpha),\r\n            //(nameof(Theme.Colors.DarkNeutral400Alpha), Theme.Colors.DarkNeutral400Alpha),\r\n            //(nameof(Theme.Colors.DarkNeutral500Alpha), Theme.Colors.DarkNeutral500Alpha),\r\n\r\n            var groups = allColors.GroupBy(GetGroupName);\r\n            \r\n            var grid = Grid(1.fr(), 1.fr(), 1.fr()).Gap(8.px());\r\n\r\n            void Render()\r\n            {\r\n                grid.Children(\r\n                        groups.OrderBy(g => g.Key == \"Neutral\" ? 0 : g.Key == \"DarkNeutral\" ? 1 : 2).ThenBy(g => g.Key)\r\n                            .Select(g =>\r\n                                VStack().Children(\r\n                                    SampleTitle(g.Key),\r\n                                    VStack().Children(g.Select(c => RenderColorStack(c.Name, c.Value)).ToArray())\r\n                                )\r\n                        ).ToArray()\r\n                    );\r\n            }\r\n            Render();\r\n            \r\n            Theme.OnThemeChanged += () => window.setTimeout(_ => Render(), 1);\r\n\r\n            _content = SectionStack()\r\n                .Title(SampleHeader(nameof(ColorsSample)))\r\n                .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Tesserae provides a comprehensive set of predefined colors that are part of the theme. These colors are accessible via the 'Theme.Colors' class and are designed to provide a consistent visual language across the application, with support for both light and dark modes.\")))\r\n                .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Prefer using these predefined colors over hardcoded hex values to ensure your application remains consistent with the theme. Use specific color ranges (e.g., Red for errors, Green for success) to convey semantic meaning. Click on any color name below to copy its C# constant name, or use the icons to copy its RGB or Hex values.\")))\r\n                .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    grid));\r\n        }\r\n\r\n        private string GetGroupName((string Name, string Value) color)\r\n        {\r\n            if (color.Name.StartsWith(\"DarkNeutral\")) return \"DarkNeutral\";\r\n            if (color.Name.StartsWith(\"Neutral\")) return \"Neutral\";\r\n\r\n            return new string(color.Name.TakeWhile(char.IsLetter).ToArray());\r\n        }\r\n\r\n        private IComponent RenderColorStack(string colorName, string colorVar)\r\n        {\r\n            var color = Color.FromString(colorVar);\r\n            var hsl= new HSLColor(color);\r\n            var textColor = \"black\";\r\n            if(hsl.Luminosity < 100)\r\n            {\r\n                textColor = \"white\";\r\n            }\r\n            return Stack().Children(\r\n                HStack().NoWrap().Background(colorVar).Children(\r\n                    Button(colorName).Foreground(textColor).NoBackground().W(10).Grow().OnClick(() =>\r\n                    {\r\n                        Clipboard.Copy($\"Theme.Colors.{colorName}\");\r\n                    }),\r\n                    Button().SetIcon(UIcons.Copy, color:textColor).Tooltip(color.ToRGB()).OnClick(() =>\r\n                    {\r\n                        Clipboard.Copy(color.ToRGB());\r\n                    }).Tooltip($\"Copy RGB Value: {color.ToRGB()}\"),\r\n                    Button().SetIcon(UIcons.Hashtag, color:textColor).OnClick(() =>\r\n                    {\r\n                        Clipboard.Copy(color.ToHex());\r\n                    }).Tooltip($\"Copy Hex Value: {color.ToHex()}\")\r\n                )\r\n            ).MB(8);\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "CommandPaletteSample": 
                            return "using System.Collections.Generic;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing H5.Core;\r\nusing static H5.Core.dom;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 31, Icon = UIcons.Command)]\r\n    public class CommandPaletteSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public CommandPaletteSample()\r\n        {\r\n            var palette = new CommandPalette(BuildActions());\r\n\r\n            var openButton = Button(\"Open Command Palette\")\r\n               .Primary()\r\n               .OnClick(() => palette.Open());\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(CommandPaletteSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"CommandPalette provides a fast and efficient way for users to navigate an application and trigger commands using only their keyboard. Inspired by modern editors and tools, it allows users to search through a list of actions and execute them with a single keystroke.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Register all major application actions in the CommandPalette. Use intuitive shortcuts and keywords to make actions easy to discover. Organize related actions into sections and utilize hierarchies for a cleaner interface. Ensure that common global actions are always easily accessible via the palette.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    TextBlock(\"Use the button below or press Cmd/Ctrl + K to open the palette.\").Small().Secondary().PB(8),\r\n                    openButton,\r\n                    TextBlock(\"Try navigating with arrow keys, Enter, Esc, and Backspace for nested items.\").Small().Secondary().PT(12)\r\n               ));\r\n        }\r\n\r\n        private static IEnumerable<CommandPaletteAction> BuildActions()\r\n        {\r\n            var navigate = new CommandPaletteAction(\"navigation\", \"Navigate\");\r\n            var home = new CommandPaletteAction(\"home\", \"Go to Home\")\r\n            {\r\n                ParentId = \"navigation\",\r\n                Perform = () => Toast().Success(\"Home\"),\r\n            };\r\n            var settings = new CommandPaletteAction(\"settings\", \"Open Settings\")\r\n            {\r\n                ParentId = \"navigation\",\r\n                Perform = () => Toast().Success(\"Settings\"),\r\n            };\r\n            var help = new CommandPaletteAction(\"help\", \"Help Center\")\r\n            {\r\n                Perform = () => Toast().Success(\"Help\"),\r\n                Shortcut = new[] { \"?\" },\r\n                Keywords = \"support docs\",\r\n                Icon = UIcons.CommentsQuestion,\r\n            };\r\n            var create = new CommandPaletteAction(\"new\", \"Create Item\")\r\n            {\r\n                Perform = () => Toast().Success(\"Create\"),\r\n                Shortcut = new[] { \"n\" },\r\n                Section = \"Actions\",\r\n                Icon = UIcons.Plus,\r\n            };\r\n            var archive = new CommandPaletteAction(\"archive\", \"Archive Item\")\r\n            {\r\n                Perform = () => Toast().Success(\"Archive\"),\r\n                Section = \"Actions\",\r\n                Icon = UIcons.Archive,\r\n            };\r\n\r\n            return new[]\r\n            {\r\n                navigate,\r\n                home,\r\n                settings,\r\n                help,\r\n                create,\r\n                archive,\r\n            };\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}\r\n";
                        case "DeferSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 0, Icon = UIcons.Spinner)]\r\n    public class DeferSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent content;\r\n\r\n        public DeferSample()\r\n        {\r\n            var stack       = SectionStack();\r\n            var countSlider = Slider(5, 0, 10, 1);\r\n\r\n            content = SectionStack()\r\n               .Title(SampleHeader(nameof(DeferSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Defer is a powerful utility for handling asynchronous UI rendering. It allows you to wrap a component that depends on an async task (like a network request). While the task is in progress, a loading message or a skeleton loader can be shown. Once the task completes, the actual component is seamlessly rendered in its place.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Defer for any component that requires remote data or slow computations. Provide a 'loadMessage'\u2014ideally a skeleton loader\u2014that mimics the final layout to reduce visual flickering. Leverage the lazy-loading nature of Defer, as the async task is only triggered when the component is first rendered. Combine Defer with observables to create highly reactive and responsive interfaces.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    Stack().Children(\r\n                        HStack().Children(\r\n                            Stack().Children(\r\n                                Label(\"Number of items:\").SetContent(countSlider.OnInput((s, e) => SetChildren(stack, s.Value)))\r\n                            ))),\r\n                    stack.HeightAuto()\r\n                ));\r\n            SetChildren(stack, 5);\r\n        }\r\n\r\n        private void SetChildren(SectionStack stack, int count)\r\n        {\r\n            stack.Clear();\r\n\r\n            for (int i = 0; i < count; i++)\r\n            {\r\n                var delay = (i + 1) * 1_000;\r\n\r\n                stack.Section(Stack().Children(\r\n                    TextBlock($\"Section {i} - delayed {i + 1} seconds\").MediumPlus().SemiBold(),\r\n                    Defer(async () =>\r\n                        {\r\n                            await Task.Delay(delay);\r\n\r\n                            return HStack().WS().HS().Children(Image(\"./assets/img/curiosity-logo.svg\").W(40).H(40), VStack().W(50).Grow().PL(8).Children(\r\n                                TextBlock(\"Wrap (Default)\").SmallPlus(),\r\n                                TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\").Width(50.percent()).PT(4),\r\n                                TextBlock(\"No Wrap\").SmallPlus().PT(4),\r\n                                Button(\"Click Me\").Primary(),\r\n                                Label(\"Icon:\").Inline().SetContent(Icon(UIcons.HelicopterSide, weight: UIconsWeight.Bold, size: TextSize.Large)),\r\n                                TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\").NoWrap().Width(50.percent()).PT(4)\r\n                            ));\r\n                        }, loadMessage:\r\n                        HStack().WS().HS().Children(Image(\"\").W(40).H(40), VStack().W(50).Grow().PL(8).Children(\r\n                            TextBlock(\"Wrap (Default)\").SmallPlus(),\r\n                            TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\").Width(50.percent()).PT(4),\r\n                            TextBlock(\"No Wrap\").SmallPlus().PT(4),\r\n                            Button(\"Click Me\").Primary(),\r\n                            Label(\"Icon:\").Inline().SetContent(Icon(UIcons.HelicopterSide, weight: UIconsWeight.Bold, size: TextSize.Large)),\r\n                            TextBlock(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\").NoWrap().Width(50.percent()).PT(4)\r\n                        )).Skeleton()\r\n                    )));\r\n            }\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return content.Render();\r\n        }\r\n    }\r\n}";
                        case "DeferWithProgressSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 10, Icon = UIcons.Spinner)]\r\n    public class DeferWithProgressSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent content;\r\n\r\n        public DeferWithProgressSample()\r\n        {\r\n            var container = VStack();\r\n            var trigger = new SettableObservable<int>(0);\r\n\r\n            content = SectionStack()\r\n               .Title(SampleHeader(nameof(DeferWithProgressSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"DeferWithProgress extends Defer by providing a way to report progress during the async operation. This is useful for long-running tasks where you want to show a progress bar or status updates.\"),\r\n                    SampleTitle(\"Basic Usage\"),\r\n                    TextBlock(\"Click the button below to start a simulated long-running task with progress reporting.\"),\r\n                    Button(\"Start Task\").Primary().OnClick(() =>\r\n                    {\r\n                        container.Clear();\r\n                        container.Add(\r\n                            DeferWithProgress(async (reportProgress) =>\r\n                            {\r\n                                for (int i = 0; i <= 100; i+=10)\r\n                                {\r\n                                    reportProgress(i / 100f, $\"Processing step {i/10} of 10...\");\r\n                                    await Task.Delay(500);\r\n                                }\r\n                                return TextBlock(\"Task Completed Successfully!\").Success().SemiBold().P(10);\r\n                            }) // Using default loadMessageGenerator\r\n                        );\r\n                    }),\r\n                    container\r\n                ))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage with Observables\"),\r\n                    TextBlock(\"DeferWithProgress can also observe values and refresh when they change, passing the observed values to the async generator.\"),\r\n                    Button(\"Trigger Refresh\").Primary().OnClick(() => trigger.Value++),\r\n                    DeferWithProgress(trigger, async (val, progress) =>\r\n                    {\r\n                        progress(0, \"Starting...\");\r\n                        await Task.Delay(500);\r\n                        progress(0.3f, \"Step 1 complete\");\r\n                        await Task.Delay(500);\r\n                        progress(0.6f, \"Step 2 complete\");\r\n                        await Task.Delay(500);\r\n                        progress(0.9f, \"Finalizing...\");\r\n                        await Task.Delay(500);\r\n                        return TextBlock($\"Loaded content for trigger value: {val}\").Success();\r\n                    })\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return content.Render();\r\n        }\r\n    }\r\n}\r\n";
                        case "EmojiSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 10, Icon = UIcons.Smile)]\r\n    public class EmojiSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public EmojiSample()\r\n        {\r\n            _content = SectionStack().S()\r\n               .Title(SampleHeader(nameof(EmojiSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Tesserae includes full support for Emojis via an integrated stylesheet and a strongly-typed enum. This allows you to easily add expressive icons to your application with complete C# IntelliSense support and consistent rendering.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Emojis to add personality and visual cues to your interface. Ensure that the Emojis used are universally understood and appropriate for the context. Avoid using Emojis as the sole way to convey critical information, as their appearance can vary slightly between platforms. Use the SearchableList below to find the exact Emoji you need.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle($\"Strongly-typed {nameof(Emoji)} enum\"),\r\n                    SearchableList(GetAllIcons().ToArray(), 25.percent(), 25.percent(), 25.percent(), 25.percent())).S(), grow: true);\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n\r\n        private IEnumerable<IconItem> GetAllIcons()\r\n        {\r\n            var     names  = Enum.GetNames(typeof(Emoji));\r\n            Emoji[] values = (Emoji[])Enum.GetValues(typeof(Emoji));\r\n\r\n            for (int i = 0; i < values.Length; i++)\r\n            {\r\n                yield return new IconItem(values[i], names[i]);\r\n            }\r\n        }\r\n\r\n        private class IconItem : ISearchableItem\r\n        {\r\n            private readonly string     _value;\r\n            private readonly IComponent component;\r\n            public IconItem(Emoji icon, string name)\r\n            {\r\n                name   = ToValidName(name.Substring(3));\r\n                _value = name + \" \" + icon.ToString();\r\n\r\n                component = HStack().WS().AlignItemsCenter().PB(4).Children(\r\n                    Icon(icon, size: TextSize.Large).MinWidth(36.px()),\r\n                    TextBlock($\"{name}\").Ellipsis().Title(icon.ToString()).W(1).Grow());\r\n            }\r\n\r\n            public bool IsMatch(string searchTerm) => _value.Contains(searchTerm);\r\n\r\n            public IComponent Render() => component;\r\n        }\r\n\r\n        //Copy of the logic in the generator code, as we don't have the enum names anymore on  Enum.GetNames(typeof(Emoji)\r\n        //Note: there are a few exceptions to this logic that were manually changed\r\n        private static string ToValidName(string icon)\r\n        {\r\n            var words = icon.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)\r\n               .Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1))\r\n               .ToArray();\r\n\r\n            var name = string.Join(\"\", words);\r\n\r\n            if (char.IsDigit(name[0]))\r\n            {\r\n                return \"_\" + name;\r\n            }\r\n            else\r\n            {\r\n                return name;\r\n            }\r\n        }\r\n    }\r\n}";
                        case "FileSelectorAndDropAreaSample": 
                            return "using Tesserae.Tests;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 20, Icon = UIcons.Folder)]\r\n    public class FileSelectorAndDropAreaSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n        public FileSelectorAndDropAreaSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(FileSelectorAndDropAreaSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"FileSelector and FileDropArea provide two different ways for users to upload files. FileSelector uses a standard button that opens the system file dialog, while FileDropArea provides a larger target area for users to drag and drop files directly into the application.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use FileSelector for simple, single-file selections in forms. Use FileDropArea when users are likely to be uploading multiple files or when a more prominent upload target is desired. Always specify the allowed file types using the 'Accepts' property. Provide immediate feedback after files are selected or dropped, such as displaying the file names or sizes.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle(\"File Selector\"),\r\n                    Label(\"Selected file size: \").Inline().SetContent(TextBlock(\"\").Var(out var size)),\r\n                    FileSelector().OnFileSelected((fs,                                                                            e) => size.Text = fs.SelectedFile.size.ToString() + \" bytes\"),\r\n                    FileSelector().SetPlaceholder(\"You must select a zip file\").Required().SetAccepts(\".zip\").OnFileSelected((fs, e) => size.Text = fs.SelectedFile.size.ToString() + \" bytes\"),\r\n                    FileSelector().SetPlaceholder(\"Please select any image\").SetAccepts(\"image/*\").OnFileSelected((fs,            e) => size.Text = fs.SelectedFile.size.ToString() + \" bytes\"),\r\n                    SampleSubTitle(\"File Drop Area\"),\r\n                    Label(\"Dropped Files: \").SetContent(Stack().Var(out var droppedFiles)),\r\n                    FileDropArea().OnFilesDropped((s, e) =>\r\n                    {\r\n                        foreach (var file in e)\r\n                        {\r\n                            droppedFiles.Add(TextBlock(file.name).Small());\r\n                        }\r\n                    }).Multiple()\r\n                ));\r\n        }\r\n\r\n        public HTMLElement Render() => _content.Render();\r\n    }\r\n}";
                        case "NodeViewSample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 0, Icon = UIcons.Network)]\r\n    public class NodeViewSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent content;\r\n\r\n        public NodeViewSample()\r\n        {\r\n            var nodeView    = NodeView().S();\r\n\r\n            nodeView.DefineNode(\"Hello World\", ib => ib.AddInput(\"inp\",  () => NodeView.Interfaces.TextInputInterface(\"Input\", \"Hi Input\"))\r\n                                                       .AddOutput(\"out\", () => NodeView.Interfaces.TextInputInterface(\"Output\", \"Hi Output\")));\r\n\r\n            nodeView.DefineNode(\"Complex\", ib => ib.AddInput(\"text\", () => NodeView.Interfaces.TextInterface(\"Input\", \"Hi Input\"))\r\n                                                   .AddInput(\"int\",  () => NodeView.Interfaces.IntegerInterface(\"Input\", 123))\r\n                                                   .AddInput(\"num\",  () => NodeView.Interfaces.NumberInterface(\"Input\", 3.14))\r\n                                                   .AddInput(\"btn\",  () => NodeView.Interfaces.ButtonInterface(\"Input\", () => Toast().Information(\"Hi!\")))\r\n                                                   .AddInput(\"chk\",  () => NodeView.Interfaces.CheckboxInterface(\"Input\", false))\r\n                                                   .AddInput(\"sel\",  () => NodeView.Interfaces.SelectInterface(\"Input\", \"A\", new ReadOnlyArray<string>(new[] { \"A\", \"B\", \"C\" })))\r\n                                                   .AddInput(\"sld\",  () => NodeView.Interfaces.SliderInterface(\"Input\", 0.5, 0, 1))\r\n                                                   .AddOutput(\"out\", () => NodeView.Interfaces.TextInterface(\"Output\", \"Hi Output\")));\r\n\r\n            nodeView.DefineDynamicNode(\"Dynamic\", ib => ib.AddInput(\"inp\", () => NodeView.Interfaces.IntegerInterface(\"Output Count\", 1)),\r\n                                                  (inputState, outputState, ib) =>\r\n                                                  {\r\n                                                      var inputCount = inputState[\"inp\"].As<int>();\r\n                                                      for(int i  = 0; i < inputCount; i++)\r\n                                                      {\r\n                                                          ib.AddOutput($\"out-{i}\", () => NodeView.Interfaces.TextInterface($\"out-{i}\", i.ToString()));\r\n                                                      }\r\n                                                  });\r\n\r\n\r\n            var textArea = TextArea().WS().H(10).Grow();\r\n\r\n            nodeView.OnChange(v => textArea.Text = v.GetJsonState(true));\r\n\r\n            textArea.OnBlur((ta, ev) => nodeView.SetState(ta.Text));\r\n\r\n            content = SectionStack()\r\n               .Title(SampleHeader(nameof(NodeViewSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"NodeView is a powerful utility for creating node-based visual editors and data flows. It allows you to define custom node types with various input and output interfaces, enabling users to build complex logic or data pipelines graphically.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use NodeView for scenarios where users need to define relationships or workflows. Keep node definitions logical and consistent. Provide descriptive names for inputs and outputs. Utilize dynamic nodes when the node structure needs to adapt based on its internal state or external data.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SplitView().SplitInMiddle().Resizable().H(600).WS().Left(nodeView).Right(VStack().S().Children(Label(\"JSON State\"), textArea))));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return content.Render();\r\n        }\r\n    }\r\n}";
                        case "ThemeColorsSample": 
                            return "using System;\r\nusing System.Collections.Generic;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 10, Icon = UIcons.Palette)]\r\n    public class ThemeColorsSample : IComponent, ISample\r\n    {\r\n        private IComponent _content;\r\n\r\n\r\n        public static void DumpTheme()\r\n        {\r\n            foreach (var (fromC, toC) in new[]\r\n                {\r\n                    (\"tss-default-background-color-root\", \"tss-default-background-hover-color-root\"),\r\n                    (\"tss-default-background-color-root\", \"tss-default-background-active-color-root\"),\r\n\r\n                    (\"tss-default-foreground-color-root\", \"tss-default-foreground-hover-color-root\"),\r\n                    (\"tss-default-foreground-color-root\", \"tss-default-foreground-active-color-root\"),\r\n\r\n                    (\"tss-primary-background-color-root\", \"tss-primary-border-color-root\"),\r\n                    (\"tss-primary-background-color-root\", \"tss-primary-background-hover-color-root\"),\r\n                    (\"tss-primary-background-color-root\", \"tss-primary-background-active-color-root\"),\r\n\r\n                    (\"tss-primary-foreground-color-root\", \"tss-primary-foreground-hover-color-root\"),\r\n                    (\"tss-primary-foreground-color-root\", \"tss-primary-foreground-active-color-root\"),\r\n\r\n                    (\"tss-danger-background-color-root\", \"tss-danger-border-color-root\"),\r\n                    (\"tss-danger-background-color-root\", \"tss-danger-background-hover-color-root\"),\r\n                    (\"tss-danger-background-color-root\", \"tss-danger-background-active-color-root\"),\r\n\r\n                    (\"tss-danger-foreground-color-root\", \"tss-danger-foreground-hover-color-root\"),\r\n                    (\"tss-danger-foreground-color-root\", \"tss-danger-foreground-active-color-root\"),\r\n\r\n                    (\"tss-scrollbar-track-color\", \"tss-scrollbar-track-hidden-color\"),\r\n                    (\"tss-scrollbar-track-color\", \"tss-scrollbar-thumb-color\"),\r\n                    (\"tss-scrollbar-track-color\", \"tss-scrollbar-thumb-hidden-color\"),\r\n\r\n                    (\"tss-default-border-color-root\", \"tss-default-background-color-root\"),\r\n                    (\"tss-dark-border-color-root\", \"tss-default-background-color-root\"),\r\n                    (\"tss-default-separator-color-root\", \"tss-default-background-color-root\"),\r\n                    (\"tss-progress-background-color-root\", \"tss-default-background-color-root\"),\r\n                    (\"tss-link-color-root\", \"tss-primary-background-color-root\"),\r\n                    (\"tss-tooltip-background-color-root\", \"tss-default-foreground-color-root\"),\r\n                    (\"tss-tooltip-foreground-color-root\", \"tss-default-background-color-root\"),\r\n                    (\"tss-tooltip-background-color-root\", \"tss-default-background-color-root\"),\r\n                    (\"tss-tooltip-foreground-color-root\", \"tss-default-foreground-color-root\"),\r\n                })\r\n            {\r\n                console.log($\"{(Theme.IsDark ? \"DARK\" : \"LIGHT\")}: {fromC} to {toC}: {LightDiff(fromC, toC):n1}\");\r\n            }\r\n        }\r\n\r\n        public static double LightDiff(string from, string to)\r\n        {\r\n            string fromVar = Color.EvalVar(\"var(--\" + from + \")\");\r\n            string toVar   = Color.EvalVar(\"var(--\" + to   + \")\");\r\n\r\n            if (!fromVar.Contains(\"(\"))\r\n            {\r\n                fromVar = \"rgb(\" + fromVar + \")\";\r\n            }\r\n\r\n            if (!toVar.Contains(\"(\"))\r\n            {\r\n                toVar = \"rgb(\" + toVar + \")\";\r\n            }\r\n            var fromColor = (HSLColor)Color.FromString(fromVar);\r\n            var toColor   = (HSLColor)Color.FromString(toVar);\r\n            return toColor.Luminosity - fromColor.Luminosity;\r\n        }\r\n\r\n        public ThemeColorsSample()\r\n        {\r\n            var currentTheme    = Theme.IsLight;\r\n            var primaryLight    = new SettableObservable<Color>();\r\n            var backgroundLight = new SettableObservable<Color>();\r\n            var primaryDark     = new SettableObservable<Color>();\r\n            var backgroundDark  = new SettableObservable<Color>();\r\n\r\n            var combined = new CombinedObservable<Color, Color, Color, Color>(primaryLight, primaryDark, backgroundLight, backgroundDark);\r\n\r\n            var cpPrimaryLight    = ColorPicker().OnInput((cp, ev) => primaryLight.Value = cp.Color);\r\n            var cpPrimaryDark     = ColorPicker().OnInput((cp, ev) => primaryDark.Value = cp.Color);\r\n            var cpBackgroundLight = ColorPicker().OnInput((cp, ev) => backgroundLight.Value = cp.Color);\r\n            var cpBackgroundDark  = ColorPicker().OnInput((cp, ev) => backgroundDark.Value = cp.Color);\r\n\r\n            Theme.Light();\r\n\r\n            window.setTimeout((_) =>\r\n            {\r\n\r\n                primaryLight.Value    = Color.FromString(Color.EvalVar(Theme.Primary.Background));\r\n                backgroundLight.Value = Color.FromString(Color.EvalVar(Theme.Default.Background));\r\n\r\n                Theme.Dark();\r\n\r\n                window.setTimeout((__) =>\r\n                {\r\n                    primaryDark.Value    = Color.FromString(Color.EvalVar(Theme.Primary.Background));\r\n                    backgroundDark.Value = Color.FromString(Color.EvalVar(Theme.Default.Background));\r\n                    Theme.IsLight        = currentTheme;\r\n\r\n\r\n                    cpPrimaryLight.Color    = primaryLight.Value;\r\n                    cpPrimaryDark.Color     = primaryDark.Value;\r\n                    cpBackgroundLight.Color = backgroundLight.Value;\r\n                    cpBackgroundDark.Color  = backgroundDark.Value;\r\n\r\n                    combined.ObserveFutureChanges(v =>\r\n                    {\r\n                        Theme.SetPrimary(v.first, v.second);\r\n                        Theme.SetBackground(v.third, v.forth);\r\n                    });\r\n\r\n                }, 1);\r\n            }, 1);\r\n\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ThemeColorsSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"ThemeColors allows for real-time inspection and customization of the application's theme. It provides a detailed view of the primary, secondary, and semantic colors used throughout the UI, and allows you to experiment with different primary and background color combinations for both light and dark modes.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use this sample to verify the accessibility and contrast of your theme choices. Ensure that primary and background colors provide sufficient contrast for readability in both light and dark themes. Changes made here are applied immediately to the entire application, allowing for rapid prototyping of different brand identities.\")))\r\n               .Section(\r\n                    Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        DetailsList<ColorListItem>(\r\n                                DetailsListColumn(title: \"ThemeName\",        width: 120.px()),\r\n                                DetailsListColumn(title: \"Background\",       width: 160.px()),\r\n                                DetailsListColumn(title: \"Foreground\",       width: 160.px()),\r\n                                DetailsListColumn(title: \"Border\",           width: 160.px()),\r\n                                DetailsListColumn(title: \"BackgroundActive\", width: 160.px()),\r\n                                DetailsListColumn(title: \"BackgroundHover\",  width: 160.px()),\r\n                                DetailsListColumn(title: \"ForegroundActive\", width: 160.px()),\r\n                                DetailsListColumn(title: \"ForegroundHover\",  width: 160.px()))\r\n                           .Compact()\r\n                           .Height(500.px())\r\n                           .WithListItems(new[]\r\n                            {\r\n                                new ColorListItem(\"Default\"),\r\n                                new ColorListItem(\"Primary\"),\r\n                                new ColorListItem(\"Secondary\"),\r\n                                new ColorListItem(\"Success\"),\r\n                                new ColorListItem(\"Danger\")\r\n                            })\r\n                           .SortedBy(\"Name\"),\r\n                        Label(\"Primary Light\").Inline().SetContent(cpPrimaryLight),\r\n                        Label(\"Primary Dark\").Inline().SetContent(cpPrimaryDark),\r\n                        Label(\"Background Light\").Inline().SetContent(cpBackgroundLight),\r\n                        Label(\"Background Dark\").Inline().SetContent(cpBackgroundDark)\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n\r\n\r\n        public class ColorListItem : IDetailsListItem<ColorListItem>\r\n        {\r\n            public string ThemeName { get; }\r\n\r\n            public static Dictionary<string, Dictionary<string, string>> Mapping = new Dictionary<string, Dictionary<string, string>>()\r\n            {\r\n                {\r\n                    \"Default\", new Dictionary<string, string>\r\n                    {\r\n                        { nameof(Theme.Default.Background), Theme.Default.Background },\r\n                        { nameof(Theme.Default.Foreground), Theme.Default.Foreground },\r\n                        { nameof(Theme.Default.Border), Theme.Default.Border },\r\n                        { nameof(Theme.Default.BackgroundActive), Theme.Default.BackgroundActive },\r\n                        { nameof(Theme.Default.BackgroundHover), Theme.Default.BackgroundHover },\r\n                        { nameof(Theme.Default.ForegroundActive), Theme.Default.ForegroundActive },\r\n                        { nameof(Theme.Default.ForegroundHover), Theme.Default.ForegroundHover },\r\n\r\n                    }\r\n                },\r\n                {\r\n                    \"Primary\", new Dictionary<string, string>\r\n                    {\r\n                        { nameof(Theme.Primary.Background), Theme.Primary.Background },\r\n                        { nameof(Theme.Primary.Foreground), Theme.Primary.Foreground },\r\n                        { nameof(Theme.Primary.Border), Theme.Primary.Border },\r\n                        { nameof(Theme.Primary.BackgroundActive), Theme.Primary.BackgroundActive },\r\n                        { nameof(Theme.Primary.BackgroundHover), Theme.Primary.BackgroundHover },\r\n                        { nameof(Theme.Primary.ForegroundActive), Theme.Primary.ForegroundActive },\r\n                        { nameof(Theme.Primary.ForegroundHover), Theme.Primary.ForegroundHover },\r\n                    }\r\n                },\r\n                {\r\n                    \"Secondary\", new Dictionary<string, string>()\r\n                    {\r\n                        { nameof(Theme.Secondary.Background), Theme.Secondary.Background },\r\n                        { nameof(Theme.Secondary.Foreground), Theme.Secondary.Foreground },\r\n//                {nameof(Theme.Secondary.Border), Theme.Secondary.Border},\r\n//                {nameof(Theme.Secondary.BackgroundActive), Theme.Secondary.BackgroundActive},\r\n//                {nameof(Theme.Secondary.BackgroundHover), Theme.Secondary.BackgroundHover},\r\n//                {nameof(Theme.Secondary.ForegroundActive), Theme.Secondary.ForegroundActive},\r\n//                {nameof(Theme.Secondary.ForegroundHover), Theme.Secondary.ForegroundHover},   \r\n                    }\r\n                },\r\n                {\r\n                    \"Danger\", new Dictionary<string, string>()\r\n                    {\r\n                        { nameof(Theme.Danger.Background), Theme.Danger.Background },\r\n                        { nameof(Theme.Danger.Foreground), Theme.Danger.Foreground },\r\n                        { nameof(Theme.Danger.Border), Theme.Danger.Border },\r\n                        { nameof(Theme.Danger.BackgroundActive), Theme.Danger.BackgroundActive },\r\n                        { nameof(Theme.Danger.BackgroundHover), Theme.Danger.BackgroundHover },\r\n                        { nameof(Theme.Danger.ForegroundActive), Theme.Danger.ForegroundActive },\r\n                        { nameof(Theme.Danger.ForegroundHover), Theme.Danger.ForegroundHover },\r\n                    }\r\n                },\r\n                {\r\n                    \"Success\", new Dictionary<string, string>\r\n                    {\r\n                        { nameof(Theme.Success.Background), Theme.Success.Background },\r\n                        { nameof(Theme.Success.Foreground), Theme.Success.Foreground },\r\n                        { nameof(Theme.Success.Border), Theme.Success.Border },\r\n                        { nameof(Theme.Success.BackgroundActive), Theme.Success.BackgroundActive },\r\n                        { nameof(Theme.Success.BackgroundHover), Theme.Success.BackgroundHover },\r\n                        { nameof(Theme.Success.ForegroundActive), Theme.Success.ForegroundActive },\r\n                        { nameof(Theme.Success.ForegroundHover), Theme.Success.ForegroundHover },\r\n                    }\r\n                }\r\n            };\r\n\r\n            public ColorListItem(string themeName)\r\n            {\r\n                ThemeName = themeName;\r\n            }\r\n\r\n\r\n            private static IComponent ColorSquare(string color)\r\n            {\r\n                if (string.IsNullOrWhiteSpace(color))\r\n                {\r\n                    return Raw(Div(_(styles: (s) =>\r\n                    {\r\n                        s.width  = \"50px\";\r\n                        s.height = \"49px\";\r\n                        //                        s.boxShadow = \"1px 1px 1px 1px lightgrey\";\r\n                    })));\r\n                }\r\n\r\n                return Raw(Div(_(styles: (s) =>\r\n                {\r\n                    s.width           = \"50px\";\r\n                    s.height          = \"49px\";\r\n                    s.backgroundColor = color;\r\n                    s.color           = color;\r\n                    s.borderColor     = color;\r\n                    s.boxShadow       = \"1px 1px 1px 1px lightgrey\";\r\n                })));\r\n            }\r\n\r\n            public int CompareTo(ColorListItem other, string columnSortingKey)\r\n            {\r\n                return 0;\r\n            }\r\n\r\n            public bool EnableOnListItemClickEvent => false;\r\n\r\n            public void OnListItemClick(int listItemIndex)\r\n            {\r\n                //                throw new NotImplementedException();\r\n                //TODO pius: pn click copy color to clipboard\r\n\r\n                Toast().Information(listItemIndex.ToString());\r\n            }\r\n\r\n            public IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> createGridCellExpression)\r\n            {\r\n                yield return createGridCellExpression(columns[0], () => TextBlock(ThemeName));\r\n                yield return createGridCellExpression(columns[1], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault(\"Background\",       \"\")));\r\n                yield return createGridCellExpression(columns[2], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault(\"Foreground\",       \"\")));\r\n                yield return createGridCellExpression(columns[3], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault(\"Border\",           \"\")));\r\n                yield return createGridCellExpression(columns[4], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault(\"BackgroundActive\", \"\")));\r\n                yield return createGridCellExpression(columns[5], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault(\"BackgroundHover\",  \"\")));\r\n                yield return createGridCellExpression(columns[6], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault(\"ForegroundActive\", \"\")));\r\n                yield return createGridCellExpression(columns[7], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault(\"ForegroundHover\",  \"\")));\r\n            }\r\n        }\r\n    }\r\n}";
                        case "TippySample": 
                            return "using System;\r\nusing System.Threading.Tasks;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 0, Icon = UIcons.CommentInfo)]\r\n    public class TippySample : IComponent, ISample\r\n    {\r\n        private readonly IComponent content;\r\n\r\n        public TippySample()\r\n        {\r\n            var stack       = SectionStack();\r\n            var countSlider = Slider(5, 0, 10, 1);\r\n\r\n            var size = new SettableObservable<int>();\r\n            var deferedWithChangingSize = DeferSync(size, sz => Button($\"Height = {sz:n0}px\").H(sz)).WS();\r\n\r\n            content = SectionStack()\r\n               .Title(SampleHeader(nameof(TippySample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Tippy is the underlying engine for tooltips and popovers in Tesserae. It provides a flexible way to attach rich, interactive content to any component, with support for various animations, placements, and trigger events.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use tooltips to provide additional context or information without cluttering the main UI. Keep text tooltips brief and focused. For interactive tooltips, ensure the content is easy to use and provides clear affordances. Utilize animations sparingly to enhance the user experience without being distracting. Always consider the placement of the tooltip to ensure it doesn't obscure relevant content.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    VStack().Children(\r\n                        Button(\"Hover me\").W(200).Tooltip(\"This is a simple text tooltip\"),\r\n                        Button(\"Animated Tooltip\").W(200).Tooltip(\"This is a simple text tooltip with animations\", TooltipAnimation.ShiftAway),\r\n                        Button(\"Interactive Tooltip\").W(200).Tooltip(Button(\"Click me\").OnClick(() => Toast().Success(\"You clicked!\")), interactive: true),\r\n                        Button(\"Defers on Tooltips\").W(200).Tooltip(deferedWithChangingSize),\r\n                        Button(\"Nested Tooltips\").W(200).Tooltip(Button(\"Click me\").OnClick((b1, _) => Tippy.ShowFor(b1, Button(\"Click me\").OnClick(() => Toast().Success(\"You clicked!\")), out var _)), interactive: true)\r\n                    )));\r\n\r\n            content.WhenMounted(() =>\r\n            {\r\n                var rng = new Random();\r\n                var repeat = window.setInterval(_ =>\r\n                {\r\n                    size.Value = rng.Next(0, 10) * 50 + 50;\r\n                }, 1000);\r\n                content.WhenRemoved(() => window.clearInterval(repeat));\r\n            });\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return content.Render();\r\n        }\r\n    }\r\n}";
                        case "ToastSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 20, Icon = UIcons.BreadSlice)]\r\n    public class ToastSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public ToastSample()\r\n        {\r\n            _content = SectionStack()\r\n               .Title(SampleHeader(nameof(ToastSample)))\r\n               .Section(Stack().WidthStretch().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Toasts are short-lived, non-intrusive notifications that provide feedback about an operation. They appear temporarily on the screen and then disappear automatically, making them ideal for success messages, warnings, or simple information updates.\")))\r\n               .Section(Stack().WidthStretch().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use Toasts for brief, informative messages that don't require user action. Keep the text short and recognizable. Ensure the Toast duration is long enough to be read but short enough not to become an annoyance. Avoid overloading the user with too many simultaneous Toasts. For critical errors that require immediate attention or user interaction, use a Dialog or Modal instead.\")))\r\n               .Section(\r\n                    Stack().WidthStretch().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        SampleSubTitle(\"Toasts top-right (default)\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().Information(\"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().Success(\"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().Warning(\"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().Error(\"Error!\"))),\r\n                        SampleSubTitle(\"Toasts top left\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().TopLeft().Information(\"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().TopLeft().Success(\"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().TopLeft().Warning(\"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().TopLeft().Error(\"Error!\"))),\r\n                        SampleSubTitle(\"Toasts bottom right\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().BottomRight().Information(\"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().BottomRight().Success(\"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().BottomRight().Warning(\"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().BottomRight().Error(\"Error!\"))),\r\n                        SampleSubTitle(\"Toasts bottom left\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().BottomLeft().Information(\"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().BottomLeft().Success(\"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().BottomLeft().Warning(\"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().BottomLeft().Error(\"Error!\"))),\r\n                        SampleSubTitle(\"Toasts top center with title\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().TopCenter().Information(\"This is a title\", \"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().TopCenter().Success(\"This is a title\", \"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().TopCenter().Warning(\"This is a title\", \"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().TopCenter().Error(\"This is a title\", \"Error!\"))),\r\n                        SampleSubTitle(\"Toasts top full with title\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().TopFull().Information(\"This is a title\", \"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().TopFull().Success(\"This is a title\", \"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().TopFull().Warning(\"This is a title\", \"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().TopFull().Error(\"This is a title\", \"Error!\"))),\r\n                        SampleSubTitle(\"Toasts bottom center with title\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().BottomCenter().Information(\"This is a title\", \"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().BottomCenter().Success(\"This is a title\", \"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().BottomCenter().Warning(\"This is a title\", \"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().BottomCenter().Error(\"This is a title\", \"Error!\"))),\r\n                        SampleSubTitle(\"Toasts bottom full with title\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info\").OnClick(() => Toast().BottomFull().Information(\"This is a title\", \"Info!\")),\r\n                            Button().SetText(\"Success\").OnClick(() => Toast().BottomFull().Success(\"This is a title\", \"Success!\")),\r\n                            Button().SetText(\"Warning\").OnClick(() => Toast().BottomFull().Warning(\"This is a title\", \"Warning!\")),\r\n                            Button().SetText(\"Error\").OnClick(() => Toast().BottomFull().Error(\"This is a title\", \"Error!\"))),\r\n                        SampleSubTitle(\"Toast as banner\"),\r\n                        HStack().Children(\r\n                            Button().SetText(\"Info on top\").OnClick(() => Toast().TopFull().Banner().Information(\"This is a banner\", \"Info!\")),\r\n                            Button().SetText(\"Success on top\").OnClick(() => Toast().TopFull().Banner().Success(\"This is a banner\", \"Success!\")),\r\n                            Button().SetText(\"Warning on bottom\").OnClick(() => Toast().BottomFull().Banner().Warning(\"This is a banner\", \"Warning!\")),\r\n                            Button().SetText(\"Error on bottom\").OnClick(() => Toast().BottomFull().Banner().Error(\"This is a banner\", \"Error!\")))\r\n                    ));\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n    }\r\n}";
                        case "UIconsSample": 
                            return "using System;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.UI;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing Tesserae.Tests;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 10, Icon = UIcons.Picture)]\r\n    public class UIconsSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent _content;\r\n\r\n        public UIconsSample()\r\n        {\r\n            //TODO: Add dropwdown to select icon weight\r\n            _content = SectionStack().S()\r\n               .Title(SampleHeader(nameof(UIconsSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"UIcons provide a massive collection of high-quality icons integrated directly into Tesserae. They are accessible through a strongly-typed enum, offering full IntelliSense support and ensuring that your application's iconography is consistent and easily maintainable.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Use icons to provide visual context and improve the scanability of your UI. Choose icons that are widely recognized and relevant to the action or content they represent. Maintain consistency in icon style and weight throughout your application. Use the SearchableList below to explore the thousands of available icons.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Usage\"),\r\n                    SampleSubTitle($\"Strongly-typed {nameof(UIcons)} enum\"),\r\n                    SearchableList(GetAllIcons().ToArray(), 25.percent(), 25.percent(), 25.percent(), 25.percent())).S(), grow: true);\r\n        }\r\n\r\n        public HTMLElement Render()\r\n        {\r\n            return _content.Render();\r\n        }\r\n\r\n        private IEnumerable<IconItem> GetAllIcons()\r\n        {\r\n            var      names  = Enum.GetNames(typeof(UIcons));\r\n            UIcons[] values = (UIcons[])Enum.GetValues(typeof(UIcons));\r\n\r\n            for (int i = 0; i < values.Length; i++)\r\n            {\r\n                yield return new IconItem(values[i], names[i]);\r\n            }\r\n        }\r\n\r\n        private class IconItem : ISearchableItem\r\n        {\r\n            private readonly string     _value;\r\n            private readonly IComponent component;\r\n            public IconItem(UIcons icon, string name)\r\n            {\r\n                name   = ToValidName(name.Substring(6));\r\n                _value = name + \" \" + icon.ToString();\r\n\r\n                component = HStack().WS().AlignItemsCenter().PB(4).Children(\r\n                    Icon(icon, size: TextSize.Large).MinWidth(36.px()),\r\n                    TextBlock($\"{name}\").Ellipsis().Title(icon.ToString()).W(1).Grow());\r\n\r\n            }\r\n\r\n            public bool IsMatch(string searchTerm) => _value.Contains(searchTerm);\r\n\r\n            public IComponent Render() => component;\r\n        }\r\n\r\n\r\n        //Copy of the logic in the generator code, as we don't have the enum names anymore on  Enum.GetNames(typeof(LineAwesome))\r\n        private static string ToValidName(string icon)\r\n        {\r\n            var words = icon.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)\r\n               .Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1))\r\n               .ToArray();\r\n\r\n            var name = string.Join(\"\", words);\r\n\r\n            if (char.IsDigit(name[0]))\r\n            {\r\n                return \"_\" + name;\r\n            }\r\n            else\r\n            {\r\n                return name;\r\n            }\r\n        }\r\n    }\r\n}";
                        case "ValidatorSample": 
                            return "using Tesserae.Tests;\r\nusing static H5.Core.dom;\r\nusing static Tesserae.Tests.Samples.SamplesHelper;\r\nusing static Tesserae.UI;\r\n\r\nnamespace Tesserae.Tests.Samples\r\n{\r\n    [SampleDetails(Group = \"Utilities\", Order = 0, Icon = UIcons.Check)]\r\n    public class ValidatorSample : IComponent, ISample\r\n    {\r\n        private readonly IComponent content;\r\n        public ValidatorSample()\r\n        {\r\n            var looksValidSoFar = TextBlock(\"?\");\r\n            var validator       = Validator().OnValidation(validity => looksValidSoFar.Text = validity == ValidationState.Invalid ? \"Something is not ok \u274c\" : \"Everything is fine so far \u2714\");\r\n\r\n            // Note: The \"Required()\" calls on these components only marks them visually as being required - if they must have values then that must be accounted for in their Validation(..) logic\r\n            var textBoxThatMustBeNonEmpty        = TextBox(\"\").Required();\r\n            var textBoxThatMustBePositiveInteger = TextBox(\"\").Required();\r\n            textBoxThatMustBeNonEmpty.Validation(tb => tb.Text.Length == 0 ? \"must enter a value\" : textBoxThatMustBeNonEmpty.Text == textBoxThatMustBePositiveInteger.Text ? \"duplicated  values\" : null, validator);\r\n            textBoxThatMustBePositiveInteger.Validation(tb => Validation.NonZeroPositiveInteger(tb) ?? (textBoxThatMustBeNonEmpty.Text == textBoxThatMustBePositiveInteger.Text ? \"duplicated values\" : null), validator);\r\n\r\n\r\n            var dropdown = Dropdown().Items(DropdownItem(\"\"), DropdownItem(\"Item 1\"), DropdownItem(\"Item 2\")).Required().Validation(dd => string.IsNullOrWhiteSpace(dd.SelectedText) ? \"must select an item\" : null, validator);\r\n\r\n            content = SectionStack()\r\n               .Title(SampleHeader(nameof(ValidatorSample)))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Overview\"),\r\n                    TextBlock(\"Validator is a utility component that aggregates the validation state of multiple UI components. It provides a centralized way to monitor whether a form or a set of inputs is valid, making it easy to provide real-time feedback to users and prevent the submission of incorrect data.\")))\r\n               .Section(Stack().Children(\r\n                    SampleTitle(\"Best Practices\"),\r\n                    TextBlock(\"Register all related input components with a single Validator. Use clear and descriptive validation messages that help users correct errors. Avoid showing validation errors immediately on form load; instead, allow users to interact with the fields first. Use the Validator's state to enable or disable primary actions like 'Submit' or 'Save' to ensure only valid data is processed.\")))\r\n               .Section(\r\n                    Stack().Children(\r\n                        SampleTitle(\"Usage\"),\r\n                        TextBlock(\"Basic TextBox\").Medium(),\r\n                        Stack().Width(40.percent()).Padding(8.px()).Children(\r\n                            Label(\"Non-empty\").SetContent(textBoxThatMustBeNonEmpty),\r\n                            Label(\"Integer > 0 (must not match the value above)\").SetContent(textBoxThatMustBePositiveInteger),\r\n                            Label(\"Pre-filled Integer > 0 (initially valid)\").SetContent(TextBox(\"123\").Required().Validation(Validation.NonZeroPositiveInteger, validator)),\r\n                            Label(\"Pre-filled Integer > 0 (initially i  nvalid)\").SetContent(TextBox(\"xyz\").Required().Validation(Validation.NonZeroPositiveInteger, validator)),\r\n                            Label(\"Not empty with forced instant validation\").SetContent(TextBox(\"\").Required().Validation(tb => string.IsNullOrWhiteSpace(tb.Text) ? \"Can't be empty\" : null, validator, forceInitialValidation: true)),\r\n                            Label(\"Please select something\").SetContent(dropdown)\r\n                        ),\r\n                        TextBlock(\"Results Summary\").Medium(),\r\n                        Stack().Width(40.percent()).Padding(8.px()).Children(\r\n                            Label(\"Validity (this only checks fields that User has interacted with so far)\").Inline().SetContent(looksValidSoFar),\r\n                            Label(\"Test revalidation (will fail if repeated)\").SetContent(Button(\"Validate\").OnClick((s, b) => validator.Revalidate()))\r\n                        )\r\n                    )\r\n                );\r\n\r\n            // 2020-09-16 DWR: The form here follows the pattern of not disabling the submit button (the \"Validate\" button in this case), so they can enter as much or as little of it as they want and then try to submit and if they\r\n            // have left required fields unfilled (or not corrected any pre-filled invalid values) then they will THEN be informed of it. But if we wanted to disable the form submit button until the form was known to be in a valid\r\n            // state then the \"AreCurrentValuesAllValid\" method can be called after the form is rendered and the button enabled state set accordingly and then updated after each ValidationOccured event.\r\n            console.log(\"Is form initially in a valid state: \" + validator.AreCurrentValuesAllValid());\r\n        }\r\n\r\n        public HTMLElement Render() => content.Render();\r\n    }\r\n}";
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("AccordionSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("An accordion contains a list of expanders that can be toggled to reveal more information. They are useful for organizing content into manageable chunks and reducing vertical space usage when not all information needs to be visible at once."), tss.UI.TextBlock("Tesserae's Accordion component manages multiple Expanders, allowing you to control whether one or multiple sections can be open at the same time.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use accordions to organize related content that might be too long to display all at once. Ensure the header of each expander clearly describes the content within. Avoid nesting accordions within accordions as it can lead to confusion. Consider using a single Expander if you only have one block of optional content.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Accordion"), tss.UI.Accordion([tss.UI.Expander("Getting started", tss.UI.TextBlock("Use expanders to reveal details in place without navigating away.")).Expanded(), tss.UI.Expander("Configuration", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.TextBlock("You can nest any component inside an expander."), tss.UI.Button$1("Primary action").Primary()])), tss.UI.Expander("Advanced", tss.UI.TextBlock("Combine with SectionStack or Card for complex layouts."))]).AllowMultipleOpen(false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Accordion with Multiple Open Allowed"), tss.UI.Accordion([tss.UI.Expander("Section 1", tss.UI.TextBlock("Multiple sections can be open simultaneously here.")), tss.UI.Expander("Section 2", tss.UI.TextBlock("This is useful for comparing information between sections.")), tss.UI.Expander("Section 3", tss.UI.TextBlock("Just set .AllowMultipleOpen(true) on the accordion."))]).AllowMultipleOpen(true), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standalone Expander"), tss.UI.Expander("What is Tesserae?", tss.UI.TextBlock("Tesserae provides a fluent API for building UI components.")).Expanded()]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ActionButtonSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ActionButtons are a variation of the standard Button component that split the interaction into two distinct parts: a display area (typically the label and an icon) and a specific action area (typically a secondary icon on the right)."), tss.UI.TextBlock("They are useful when you want to provide a primary action while also offering a secondary, related action like opening a menu, showing a tooltip, or triggering a specific sub-task.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use ActionButtons when a component needs to perform more than one related task. The primary area should trigger the most common action, while the secondary area (the action icon) should trigger a complementary one. Clearly distinguish between the two areas visually if they perform very different tasks. Ensure that both interaction points have appropriate tooltips or labels if their purpose isn't immediately obvious.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Action Buttons"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Var(tss.ActionButton, tss.UI.ActionButton("Standard Action"), btn1).OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Information("Clicked display!");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Information("Clicked action icon!");
                }), tss.UI.ActionButton("Primary with Calendar Icon", 689, "fi-rr-", void 0, "tss-fontsize-small").Primary().OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Success("Main area clicked");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Success("Calendar icon clicked");
                }), tss.UI.ActionButton$1("Danger Action", 4343, "fi-rr-", void 0, "tss-fontsize-small", "fi-rr-", 93, void 0, "tss-fontsize-small").Danger().OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Error("Danger area clicked");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Error("Warning icon clicked");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Complex Content"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.ActionButton$2(tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.UI.Icon$2(224), tss.ICX.PL(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Move Item")), 8)]), tss.ICX.PT(tss.txt, tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("Use this to reorganize your workspace")), 4)])).OnClickDisplay(function (s, e) {
                    tss.UI.Toast().Information("Moving...");
                }).OnClickAction(function (s, e) {
                    tss.UI.Toast().Information("Configure move...");
                }), tss.UI.ActionButton("Action with Custom Tooltip").ModifyActionButton(function (btn) {
                    tss.ICX.Tooltip$1(tss.Raw, tss.UI.Raw$3(btn), "This tooltip is applied to the entire component");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dropdown Simulation"), tss.UI.ActionButton("Show Options", 103, "fi-rr-", void 0, "tss-fontsize-small").Primary().OnClickAction(function (s, e) {
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
                })]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("AvatarSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Avatars are used to represent users, teams, or entities in the system. They can display images, initials, and presence indicators."), tss.UI.TextBlock("The Persona component builds upon Avatar by adding textual information like name, role, and status, making it ideal for profile cards or contact lists.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use avatars to provide visual recognition for users. Always provide initials as a fallback for when images fail to load or aren't available. Use the appropriate size for the context\u2014smaller for lists or chat, larger for profiles. Presence indicators should be used when real-time availability information is relevant to the user's task.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Avatar Sizes and Presence"), tss.UI.TextBlock("Avatars support various sizes from XSmall to XLarge and optional presence states."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.XSmall), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.Small).Presence(Tesserae.AvatarPresence.Online), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.Medium).Presence(Tesserae.AvatarPresence.Away), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.Large).Presence(Tesserae.AvatarPresence.Busy), tss.UI.Avatar("https://cataas.com/cat", "JD").Size(Tesserae.AvatarSize.XLarge).Presence(Tesserae.AvatarPresence.Offline)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Initials Fallback"), tss.UI.TextBlock("When no image is provided, initials are displayed with a generated background color."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Avatar(void 0, "JD").Size(Tesserae.AvatarSize.Small).Presence(Tesserae.AvatarPresence.Online), tss.UI.Avatar(void 0, "AS").Size(Tesserae.AvatarSize.Medium).Presence(Tesserae.AvatarPresence.Away), tss.UI.Avatar(void 0, "KL").Size(Tesserae.AvatarSize.Large).Presence(Tesserae.AvatarPresence.Busy), tss.UI.Avatar(void 0, "MW").Size(Tesserae.AvatarSize.XLarge).Presence(Tesserae.AvatarPresence.Offline)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Persona Component"), tss.UI.TextBlock("Personas combine an avatar with descriptive text."), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Persona("Jordan Diaz", "Product Designer", "Available for collaboration", tss.UI.Avatar(void 0, "JD").Presence(Tesserae.AvatarPresence.Online)), tss.UI.Persona("Alex Smith", "Software Engineer", "Focusing...", tss.UI.Avatar(void 0, "AS").Presence(Tesserae.AvatarPresence.Busy)), tss.UI.Persona("Kelly Lee", "Project Manager", "Away", tss.UI.Avatar(void 0, "KL").Presence(Tesserae.AvatarPresence.Away))])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("BadgeSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Badges, Tags, and Chips are small visual elements used to categorize content, highlight status, or display metadata."), tss.UI.TextBlock("They come in various styles: Badges are typically static indicators, Tags are for categorization, and Chips often include interactive elements like a removal button.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use badges to call attention to small pieces of information like counts or status. Use tags for categorization where multiple labels might apply. Use chips for entities that can be removed or interacted with individually. Ensure colors are used consistently to convey meaning (e.g., red for danger/errors, green for success).")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standard Badges"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Badge("Default"), tss.UI.Badge("Primary").Primary(), tss.UI.Badge("Success").Success(), tss.UI.Badge("Warning").Warning(), tss.UI.Badge("Danger").Danger(), tss.UI.Badge("Info").Info().Outline()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Tags and Chips"), tss.UI.TextBlock("Tags and chips support icons, pill shapes, and interactive removal."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Tag("Categorization").Outline().Pill(), tss.UI.Tag("Metadata").SetIcon(tss.Icon.Transform(4105, "fi-rr-")).Outline(), tss.UI.Chip("Interactive Chip").Filled().OnRemove(function (c) {
                    tss.UI.Toast().Success("Removed chip");
                }), tss.UI.Chip("Status Chip").Success().Pill()])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("BreadcrumbSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Breadcrumbs provide a secondary navigation system that reveals a user's location in a website or web app. They allow for one-click access to any higher level in the hierarchy."), tss.UI.TextBlock("Unlike TextBreadcrumbs, this component supports more advanced configuration like custom chevrons, overflow indices, and different sizes.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Place breadcrumbs at the top of the page, above the primary content. Use them when the site hierarchy is at least two levels deep. Each breadcrumb item should represent a page or a container. The last item should represent the current location and be non-clickable. Ensure that the breadcrumbs collapse gracefully on smaller screens or when space is limited.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Breadcrumbs"), tss.ICX.PB(tss.Breadcrumb, tss.UI.Breadcrumb().Items([tss.UI.Crumb("Home").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Home");
                }), tss.UI.Crumb("Project A").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Project A");
                }), tss.UI.Crumb("Subfolder 1").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Subfolder 1");
                }), tss.UI.Crumb("Current Page")]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Responsive and Collapsed"), tss.UI.TextBlock("Breadcrumbs will collapse when the container width is restricted."), tss.ICX.PB(tss.Breadcrumb, tss.ICX.MaxWidth(tss.Breadcrumb, tss.UI.Breadcrumb(), tss.usX.px$1(250)).Items([tss.UI.Crumb("Root"), tss.UI.Crumb("Level 1"), tss.UI.Crumb("Level 2"), tss.UI.Crumb("Level 3"), tss.UI.Crumb("Final")]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Small Size and Custom Chevron"), tss.ICX.PB(tss.Breadcrumb, tss.UI.Breadcrumb().Small().SetChevron(105).Items([tss.UI.Crumb("Resources"), tss.UI.Crumb("Icons"), tss.UI.Crumb("UIcons")]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Overflow Configuration"), tss.UI.TextBlock("You can control where the overflow starts (e.g., after the second item)."), tss.ICX.MaxWidth(tss.Breadcrumb, tss.UI.Breadcrumb().SetOverflowIndex(1), tss.usX.px$1(200)).Items([tss.UI.Crumb("Home"), tss.UI.Crumb("App"), tss.UI.Crumb("Module"), tss.UI.Crumb("Feature"), tss.UI.Crumb("Detail")])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ButtonSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog."), tss.UI.TextBlock("When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Buttons should clearly communicate what will happen when the user clicks them. Use concise, specific, self-explanatory labels, usually a single word. Default buttons should always perform safe operations. For example, a default button should never delete. Use only a single line of text in the label of the button. Expose only one or two buttons to the user at a time. Show only one primary button that inherits theme color at rest state. \"Submit\", \"OK\", and \"Apply\" buttons should always be styled as primary buttons."), tss.UI.TextBlock("Avoid using generic labels like \"Ok\", especially in the case of an error. Do not place the default focus on a button that destroys data. Do not use a button to navigate to another place, use a link instead.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Buttons"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetText("Standard"), "This is a standard button").OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetText("Primary"), "This is a primary button").Primary().OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetText("Link"), "This is a link button").Link().OnClick$1(function () {
                    alert("Clicked!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Icons and States"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Confirm").SetIcon$1(876).Success().OnClick$1(function () {
                    alert("Clicked!");
                }), tss.UI.Button$1().SetText("Delete").SetIcon$1(4310).Danger().OnClick$1(function () {
                    alert("Clicked!");
                }), tss.UI.Button$1().SetText("Disabled").SetIcon$1(2581).Disabled()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Loading States"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Click to Spin").OnClickSpinWhile(function () {
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
                }, 3486, 103).OnClick(function (b, _) {
                    tss.UI.Toast().Success("Main action");
                }), tss.UI.Button$1().SetText("No Padding").NoPadding().Primary(), tss.UI.Button$1().SetText("No Border").NoBorder()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Themed Backgrounds"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Blue"), tss.UI.Theme.Colors.Blue500).OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Lime"), tss.UI.Theme.Colors.Lime500).OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Magenta"), tss.UI.Theme.Colors.Magenta500).OnClick$1(function () {
                    alert("Clicked!");
                }), tss.ISX.Background(tss.Button, tss.UI.Button$1().SetText("Yellow"), tss.UI.Theme.Colors.Yellow500).OnClick$1(function () {
                    alert("Clicked!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Buttons"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ISX.Rounded(tss.Button, tss.UI.Button$1().SetText("Small"), tss.BR.Small).Primary(), tss.ISX.Rounded(tss.Button, tss.UI.Button$1().SetText("Medium"), tss.BR.Medium).Primary(), tss.ISX.Rounded(tss.Button, tss.UI.Button$1().SetText("Full"), tss.BR.Full).Primary()])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("CarouselSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Carousels allow users to cycle through a set of related content, such as images, features, or messages. They are effective for showcasing highlights in a limited space."), tss.UI.TextBlock("The component supports any Tesserae component as a slide and provides automatic or manual navigation.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use carousels for high-impact visual content. Keep the number of slides low (typically 3-5) to ensure users can reasonably see all content. Ensure that each slide has a clear and unique message. Provide navigation controls (arrows/dots) and ensure they are accessible. For slides with text content, ensure sufficient contrast and use .PadSlides() to prevent overlapping with controls.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Text Carousel"), tss.ICX.H(tss.Carousel, tss.UI.Carousel([textSlide1, textSlide2, textSlide3]).PadSlides(), 150), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Image Gallery Carousel"), tss.ICX.H(tss.Carousel, tss.UI.Carousel([imgSlide1, imgSlide2, imgSlide3]), 300), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Carousel"), tss.ICX.H(tss.Carousel, tss.UI.Carousel([tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Interactive Slide")), tss.UI.Button$1("Click me").OnClick$1(function () {
                    tss.UI.Toast().Success("Clicked!");
                })]), 32), tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Configuration Slide")), tss.UI.CheckBox$1("Enable feature")]), 32)]).PadSlides(), 150)]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("CheckBoxSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("CheckBoxes allow users to select one or more items from a set. They can also be used to turn an option on or off."), tss.UI.TextBlock("Unlike a Toggle, which is typically used for immediate actions, a CheckBox is often used when a user needs to confirm their selection by clicking a submit button.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use CheckBoxes when users can select any number of options from a list. Clearly label each CheckBox so the user knows what they are selecting. If you have only two mutually exclusive options, consider using a ChoiceGroup (Radio buttons) or a Toggle. Don't use CheckBoxes as an on/off control for immediate actions; use a Toggle instead.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic CheckBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.CheckBox$1("Unchecked checkbox"), tss.UI.CheckBox$1("Checked checkbox").Checked(), tss.UI.CheckBox$1("Disabled checkbox").Disabled(), tss.UI.CheckBox$1("Disabled checked checkbox").Checked().Disabled()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation and States"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Required(tss.Label, tss.UI.Label$1("Required choice")).SetContent(tss.UI.CheckBox$1("I agree to the terms")), tss.ICX.Tooltip$1(tss.ChecBox, tss.UI.CheckBox$1("Checkbox with tooltip"), "More info here"), tss.UI.CheckBox$1("Triggers event").OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Checked: {0}", [H5.box(s.IsChecked, System.Boolean, System.Boolean.toString)]));
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Formatting"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Tiny(tss.ChecBox, tss.UI.CheckBox$1("Tiny")), tss.ITFX.Small(tss.ChecBox, tss.UI.CheckBox$1("Small (default)")), tss.ITFX.SmallPlus(tss.ChecBox, tss.UI.CheckBox$1("Small Plus")), tss.ITFX.Medium(tss.ChecBox, tss.UI.CheckBox$1("Medium")), tss.ITFX.Large(tss.ChecBox, tss.UI.CheckBox$1("Large")), tss.ITFX.XLarge(tss.ChecBox, tss.UI.CheckBox$1("XLarge")), tss.ITFX.XXLarge(tss.ChecBox, tss.UI.CheckBox$1("XXLarge")), tss.ITFX.Mega(tss.ChecBox, tss.UI.CheckBox$1("Mega")), tss.ITFX.Bold(tss.ChecBox, tss.UI.CheckBox$1("Bold text"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded CheckBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.ChecBox, tss.UI.CheckBox$1("Small rounded"), tss.BR.Small), tss.ISX.Rounded(tss.ChecBox, tss.UI.CheckBox$1("Medium rounded"), tss.BR.Medium), tss.ISX.Rounded(tss.ChecBox, tss.UI.CheckBox$1("Full rounded"), tss.BR.Full)])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ChoiceGroupSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ChoiceGroups, also known as radio button groups, allow users to select exactly one option from a set of mutually exclusive choices."), tss.UI.TextBlock("They emphasize all options equally, which can be useful when you want to ensure the user considers all available alternatives before making a selection.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use ChoiceGroups when there are between 2 and 7 options and screen space is available. For more than 7 options, a Dropdown is typically more efficient. List options in a logical order (e.g., most likely to least likely). Align options vertically whenever possible for better readability and localization support. Always provide a default selection if one is significantly more likely than the others, and ensure the safest option is the default if applicable.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic ChoiceGroup"), tss.UI.ChoiceGroup().Choices([tss.UI.Choice("Option A"), tss.UI.Choice("Option B"), tss.UI.Choice("Option C").Disabled(), tss.UI.Choice("Option D")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Required with Label"), tss.UI.ChoiceGroup("Select an environment").Required().Choices([tss.UI.Choice("Development"), tss.UI.Choice("Staging"), tss.UI.Choice("Production")]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Horizontal Layout"), tss.UI.ChoiceGroup("Sizes").Horizontal().Choices([tss.UI.Choice("Small"), tss.ITFX.Medium(tss.ChoiceGroup.Choice, tss.UI.Choice("Medium")), tss.ITFX.Large(tss.ChoiceGroup.Choice, tss.UI.Choice("Large"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.ChoiceGroup("Language").Choices([tss.UI.Choice("English"), tss.UI.Choice("Spanish"), tss.UI.Choice("French")]).OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0}", [s.SelectedOption.Text]));
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Formatting"), tss.UI.ChoiceGroup("Pick a style").Choices([tss.ITFX.Tiny(tss.ChoiceGroup.Choice, tss.UI.Choice("Tiny")), tss.ITFX.Small(tss.ChoiceGroup.Choice, tss.UI.Choice("Small (default)")), tss.ITFX.SmallPlus(tss.ChoiceGroup.Choice, tss.UI.Choice("Small Plus")), tss.ITFX.Medium(tss.ChoiceGroup.Choice, tss.UI.Choice("Medium")), tss.ITFX.Large(tss.ChoiceGroup.Choice, tss.UI.Choice("Large")), tss.ITFX.XLarge(tss.ChoiceGroup.Choice, tss.UI.Choice("XLarge")), tss.ITFX.XXLarge(tss.ChoiceGroup.Choice, tss.UI.Choice("XXLarge")), tss.ITFX.Mega(tss.ChoiceGroup.Choice, tss.UI.Choice("Mega")), tss.ITFX.Bold(tss.ChoiceGroup.Choice, tss.UI.Choice("Bold"))])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ColorPickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The ColorPicker allows users to select a color using the browser's native color selection widget. It returns the selected color as both a hex string and a Color object."), tss.UI.TextBlock("This component is useful for personalization settings, drawing applications, or any interface where color customization is required.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use the ColorPicker when users need to select a precise color that isn't covered by a predefined set of options. If you only need a few specific colors, consider using a ChoiceGroup with custom styling or a Dropdown instead. Always provide a default color that makes sense for the context. Ensure the picked color is validated if certain constraints apply (e.g., must be a dark color for text readability).")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic ColorPicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Pick a color").SetContent(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.ColorPicker, tss.UI.Var(tss.ColorPicker, tss.UI.ColorPicker(), cp1), tss.usX.px$1(50)), tss.UI.Var(tss.Button, tss.UI.Button$1("Apply Color"), btn1)])), tss.UI.Label$1("With default color (Blue)").SetContent(tss.ICX.Width(tss.ColorPicker, tss.UI.ColorPicker(tss.Color.FromString("#0078d4")), tss.usX.px$1(50))), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled state")).SetContent(tss.ICX.Width(tss.ColorPicker, tss.UI.ColorPicker().Disabled(), tss.usX.px$1(50)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Light color required").SetContent(tss.ICX.Width(tss.ColorPicker, tss.vX.Validation(tss.ColorPicker, tss.UI.ColorPicker(), tss.Validation.LightColor), tss.usX.px$1(50))), tss.UI.Label$1("Dark color required").SetContent(tss.ICX.Width(tss.ColorPicker, tss.vX.Validation(tss.ColorPicker, tss.UI.ColorPicker(), tss.Validation.DarkColor), tss.usX.px$1(50)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Example"), tss.UI.TextBlock("Changing the color picker below will update the button's background.")]));

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
                        return tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle(g.key()), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), g.select(H5.fn.bind(this, function (c) {
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ColorsSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Tesserae provides a comprehensive set of predefined colors that are part of the theme. These colors are accessible via the 'Theme.Colors' class and are designed to provide a consistent visual language across the application, with support for both light and dark modes.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Prefer using these predefined colors over hardcoded hex values to ensure your application remains consistent with the theme. Use specific color ranges (e.g., Red for errors, Green for success) to convey semantic meaning. Click on any color name below to copy its C# constant name, or use the icons to copy its RGB or Hex values.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), grid]));
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
                }), tss.ICX.Tooltip$1(tss.Button, tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetIcon$1(1205, textColor, "tss-fontsize-small", "fi-rr-", false), color.ToRGB()).OnClick$1(function () {
                    tss.Clipboard.Copy(color.ToRGB());
                }), System.String.format("Copy RGB Value: {0}", [color.ToRGB()])), tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1().SetIcon$1(2141, textColor, "tss-fontsize-small", "fi-rr-", false).OnClick$1(function () {
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("CommandBarSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Command Bars provide a surface for common actions related to a specific context, such as a page or a selected item in a list."), tss.UI.TextBlock("They typically contain buttons with icons and labels, and can be split into 'near' items (left-aligned) and 'far' items (right-aligned).")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Command Bars for primary actions that users perform frequently. Keep the number of items manageable; if there are too many, consider using a 'More' menu. Order items by importance or frequency of use. Group related actions together. Use 'far' items for actions that are global to the surface, such as settings or search.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Command Bar"), tss.UI.CommandBar([tss.UI.CommandBarItem("New", 3250).Primary().OnClick$1(function () {
                    tss.UI.Toast().Success("New item");
                }), tss.UI.CommandBarItem("Edit", 1529).OnClick$1(function () {
                    tss.UI.Toast().Success("Edit item");
                }), tss.UI.CommandBarItem("Share", 3648).OnClick$1(function () {
                    tss.UI.Toast().Success("Share item");
                }), tss.UI.CommandBarItem("Delete", 4310).OnClick$1(function () {
                    tss.UI.Toast().Success("Delete item");
                })]).FarItems([tss.ICX.Width(tss.SearchBox, tss.UI.SearchBox().SetPlaceholder("Search..."), tss.usX.px$1(200)), tss.UI.CommandBarItem("Settings", 3641).OnClick$1(function () {
                    tss.UI.Toast().Information("Settings clicked");
                })])]));
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
                    }, $t.Shortcut = System.Array.init(["?"], System.String), $t.Keywords = "support docs", $t.Icon = 1160, $t);
                    var create = ($t = new tss.CommandPaletteAction("new", "Create Item"), $t.Perform = function () {
                        tss.UI.Toast().Success("Create");
                    }, $t.Shortcut = System.Array.init(["n"], System.String), $t.Section = "Actions", $t.Icon = 3250, $t);
                    var archive = ($t = new tss.CommandPaletteAction("archive", "Archive Item"), $t.Perform = function () {
                        tss.UI.Toast().Success("Archive");
                    }, $t.Section = "Actions", $t.Icon = 142, $t);

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
                var palette = new tss.CommandPalette(Tesserae.Tests.Samples.CommandPaletteSample.BuildActions());

                var openButton = tss.UI.Button$1("Open Command Palette").Primary().OnClick$1(function () {
                    palette.Open();
                });

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("CommandPaletteSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("CommandPalette provides a fast and efficient way for users to navigate an application and trigger commands using only their keyboard. Inspired by modern editors and tools, it allows users to search through a list of actions and execute them with a single keystroke.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Register all major application actions in the CommandPalette. Use intuitive shortcuts and keywords to make actions easy to discover. Organize related actions into sections and utilize hierarchies for a cleaner interface. Ensure that common global actions are always easily accessible via the palette.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PB(tss.txt, tss.txtX.Secondary(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Use the button below or press Cmd/Ctrl + K to open the palette."))), 8), openButton, tss.ICX.PT(tss.txt, tss.txtX.Secondary(tss.txt, tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Try navigating with arrow keys, Enter, Esc, and Backspace for nested items."))), 12)]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ContextMenuSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ContextMenu is a flyout component that displays a list of commands triggered by user interaction, such as a right-click or a button press."), tss.UI.TextBlock("It provides a focused set of actions relevant to the current context, helping to keep the main interface clean and uncluttered. It supports nested submenus, dividers, headers, and custom component items.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use ContextMenus to surface secondary actions that are relevant to a specific element. Group related commands using dividers. Use submenus sparingly to avoid deep nesting that can be hard to navigate. Ensure that the menu remains within the viewport when opened. Always provide clear labels and icons for common actions.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Simple Context Menu"), tss.ICX.MB(tss.Button, tss.UI.Var(tss.Button, tss.UI.Button$1("Click for Menu"), btn1).OnClick(function (s, e) {
                    tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(3250), tss.ICX.ML(tss.txt, tss.UI.TextBlock("New Item"), 8)])).OnClick(function (_, __) {
                        tss.UI.Toast().Success("New Item created");
                    }), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(1867), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Open"), 8)])).OnClick(function (_, __) {
                        tss.UI.Toast().Information("Opening...");
                    }), tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$1(4310, tss.UI.Theme.Danger.Background), tss.txtX.Danger(tss.txt, tss.ICX.ML(tss.txt, tss.UI.TextBlock("Delete"), 8))])).OnClick(function (_, __) {
                        tss.UI.Toast().Error("Deleted");
                    })]).ShowFor$1(btn1.v);
                }), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Menu with Submenus and Headers"), tss.UI.Var(tss.Button, tss.UI.Button$1("Complex Menu"), btn2).OnClick(function (s, e) {
                    tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem("Actions").Header(), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(1529), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Edit"), 8)])).SubMenu(tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem("Edit Name"), tss.UI.ContextMenuItem("Edit Permissions"), tss.UI.ContextMenuItem("Edit Metadata")])), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(3648), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Share"), 8)])).SubMenu(tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem("Copy Link"), tss.UI.ContextMenuItem("Email Link")])), tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem("Advanced").Header(), tss.UI.ContextMenuItem("Properties").Disabled(), tss.UI.ContextMenuItem$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(3641), tss.ICX.ML(tss.txt, tss.UI.TextBlock("Settings"), 8)]))]).ShowFor$1(btn2.v);
                })]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("CronEditorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("CronEditor allows users to schedule tasks using a simplified UI for daily schedules, with a fallback to raw cron expressions for advanced users.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic"), tss.UI.CronEditor(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Days Selection Disabled"), tss.UI.CronEditor().DaysEnabled(false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Interval (30 mins)"), tss.UI.CronEditor().MinuteInterval(30), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Initial Value (Custom)"), tss.UI.CronEditor("*/5 * * * *"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Initially Disabled"), tss.UI.CronEditor("0 12 * * *", false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Enable Checkbox Hidden"), tss.UI.CronEditor().ShowEnableCheckbox(false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Observable"), this.GetObservableExample()]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DatePickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("DatePickers allow users to select a specific date using a browser-native date selection widget. They ensure that the input is always a valid date format."), tss.UI.TextBlock("This component is suitable for forms requiring birthdays, appointment dates, or any date-driven data entry.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use the DatePicker when users need to enter a specific date. If you need to include time as well, use the DateTimePicker instead. Always provide min and max constraints if the acceptable date range is limited. Use clear validation messages to guide users if they select an invalid date (e.g., a date in the past when only future dates are allowed).")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic DatePicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.DatePicker()), tss.UI.Label$1("Pre-selected Date (Next Week)").SetContent(tss.UI.DatePicker(System.DateTime.addDays(System.DateTime.getNow(), 7))), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.DatePicker().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Range and Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1(System.String.format("Limited Range (Between {0:d} and {1:d})", H5.box(from, System.DateTime, System.DateTime.format), H5.box(to, System.DateTime, System.DateTime.format))).SetContent(tss.UI.DatePicker().SetMin(from).SetMax(to)), tss.UI.Label$1("Step increment of 5 days").SetContent(tss.UI.DatePicker().SetStep(5))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Not in the future").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), tss.Validation.NotInTheFuture)), tss.UI.Label$1("Not in the past").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), tss.Validation.NotInThePast)), tss.UI.Label$1("Custom validation (within 2 months)").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), function (dp) {
                    return System.DateTime.lte(dp.Date, System.DateTime.addMonths(System.DateTime.getNow(), 2)) ? null : "Please choose a date less than 2 months in the future";
                }))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.DatePicker().OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected date: {0:d}", [H5.box(s.Date, System.DateTime, System.DateTime.format)]));
                })]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DateTimePickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The DateTimePicker combines date and time selection into a single component, using the browser's native widget."), tss.UI.TextBlock("It is ideal for scheduling events, setting deadlines, or any task where both the day and time are critical.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use the DateTimePicker when users need to specify a precise moment in time. Consider the user's timezone if the application handles users across different regions. Provide sensible defaults, such as the current time or a common starting point. Use min/max constraints to prevent invalid selections (e.g., booking an appointment in the past).")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic DateTimePicker"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.DateTimePicker()), tss.UI.Label$1("Pre-selected (Now)").SetContent(tss.UI.DateTimePicker(System.DateTime.getNow())), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.DateTimePicker().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1(System.String.format("Range: {0:g} to {1:g}", H5.box(from, System.DateTime, System.DateTime.format), H5.box(to, System.DateTime, System.DateTime.format))).SetContent(tss.UI.DateTimePicker().SetMin(from).SetMax(to)), tss.UI.Label$1("10-second intervals").SetContent(tss.UI.DateTimePicker().SetStep(10))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Must be in the future").SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), tss.Validation.NotInThePast$1)), tss.UI.Label$1("Within next 48 hours").SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), function (dtp) {
                    return System.DateTime.lte(dtp.DateTime, System.DateTime.addHours(System.DateTime.getNow(), 48)) ? null : "Must be within 48 hours";
                }))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.DateTimePicker().OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Selected: {0:g}", [H5.box(s.DateTime, System.DateTime, System.DateTime.format)]));
                })]));
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
                var stack = tss.UI.SectionStack();
                var countSlider = tss.UI.Slider(5, 0, 10, 1);

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DeferSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Defer is a powerful utility for handling asynchronous UI rendering. It allows you to wrap a component that depends on an async task (like a network request). While the task is in progress, a loading message or a skeleton loader can be shown. Once the task completes, the actual component is seamlessly rendered in its place.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Defer for any component that requires remote data or slow computations. Provide a 'loadMessage'\u2014ideally a skeleton loader\u2014that mimics the final layout to reduce visual flickering. Leverage the lazy-loading nature of Defer, as the async task is only triggered when the component is first rendered. Combine Defer with observables to create highly reactive and responsive interfaces.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Number of items:").SetContent(countSlider.OnInput(H5.fn.bind(this, function (s, e) {
                    this.SetChildren(stack, s.Value);
                })))])])]), tss.ICX.HeightAuto(tss.SectionStack, stack)]));
                this.SetChildren(stack, 5);
            }
        },
        methods: {
            SetChildren: function (stack, count) {
                stack.Clear();

                for (var i = 0; i < count; i = (i + 1) | 0) {
                    var delay = { v : H5.Int.mul((((i + 1) | 0)), 1000) };

                    tss.SectionStackX.Section(stack, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ITFX.SemiBold(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock(System.String.format("Section {0} - delayed {1} seconds", H5.box(i, System.Int32), H5.box(((i + 1) | 0), System.Int32))))), tss.UI.Defer$1((function ($me, delay) {
                        return H5.fn.bind($me, function () {
                            var $tcs = new H5.TCS();
                            (async () => {
                                {
                                    (await H5.toPromise(System.Threading.Tasks.Task.delay(delay.v)));

                                    return tss.ICTX.Children$6(tss.S, tss.ICX.HS(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack())), [tss.ICX.H(tss.Image, tss.ICX.W(tss.Image, tss.UI.Image$1("./assets/img/curiosity-logo.svg"), 40), 40), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.ICX.W(tss.S, tss.UI.VStack(), 50)), 8), [tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), 4), tss.ICX.PT(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), 4), tss.UI.Button$1("Click Me").Primary(), tss.UI.Label$1("Icon:").Inline().SetContent(tss.UI.Icon$2(2184, "fi-br-", "tss-fontsize-large", void 0)), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50)), 4)])]);
                                }})().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                            return $tcs.task;
                        });
                    })(this, delay), tss.ICTX.Children$6(tss.S, tss.ICX.HS(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack())), [tss.ICX.H(tss.Image, tss.ICX.W(tss.Image, tss.UI.Image$1(""), 40), 40), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.ICX.W(tss.S, tss.UI.VStack(), 50)), 8), [tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), 4), tss.ICX.PT(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), 4), tss.UI.Button$1("Click Me").Primary(), tss.UI.Label$1("Icon:").Inline().SetContent(tss.UI.Icon$2(2184, "fi-br-", "tss-fontsize-large", void 0)), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50)), 4)])]).Skeleton())]));
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

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DeferWithProgressSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("DeferWithProgress extends Defer by providing a way to report progress during the async operation. This is useful for long-running tasks where you want to show a progress bar or status updates."), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Basic Usage"), tss.UI.TextBlock("Click the button below to start a simulated long-running task with progress reporting."), tss.UI.Button$1("Start Task").Primary().OnClick$1(function () {
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
                }), container])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage with Observables"), tss.UI.TextBlock("DeferWithProgress can also observe values and refresh when they change, passing the observed values to the async generator."), tss.UI.Button$1("Trigger Refresh").Primary().OnClick$1(function () {
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
                })]));
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
                                }).ToArray(tss.txt), tss.ICX.PR(tss.Icon, tss.UI.Icon$2(876), 8));
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


                this._content = tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DeltaComponent")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("DeltaComponent updates its DOM tree to match a new component's DOM tree using a diff algorithm. It detects text appends and adds them as new spans to avoid full re-rendering."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [typing, typingWithComponents, resetBtn]), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Output"), deltaComponent, Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Shadow DOM"), tss.UI.TextBlock("This DeltaComponent renders its content inside a Shadow DOM root."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [shadowTyping, shadowResetBtn]), shadowDeltaComponent]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DetailsListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("DetailsList is a robust way to display an information-rich collection of items. It supports sorting, grouping, filtering, and pagination."), tss.UI.TextBlock("It is classically used for file explorers, database record views, or any scenario where information density is critical.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use DetailsList when users need to compare items across multiple metadata fields. Display columns in order of importance from left to right. Provide ample default width for each column to avoid unnecessary truncation. Use compact mode when vertical space is limited or when displaying very large datasets. Always provide a clear empty state message if the list contains no items.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standard File List"), tss.UI.TextBlock("A list with textual rows, supporting sorting and custom column widths."), tss.ICX.MB(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1727), tss.usX.px$1(32), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.px$1(350), true, true, "FileName", void 0), tss.UI.DetailsListColumn("Date Modified", tss.usX.px$1(170), false, true, "DateModified", void 0), tss.UI.DetailsListColumn("Modified By", tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn("File Size", tss.usX.px$1(120), false, true, "FileSize", void 0)]), tss.usX.px$1(400)).WithListItems(this.GetDetailsListItems()).SortedBy("FileName"), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Responsive Widths"), tss.UI.TextBlock("Using percentage-based widths with max-width constraints."), tss.ICX.MB(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.WidthStretch(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1727), tss.usX.px$1(64), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.percent$1(40), true, true, "FileName", void 0), tss.UI.DetailsListColumn$1("Date Modified", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "DateModified", void 0), tss.UI.DetailsListColumn$1("Modified By", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn$1("File Size", tss.usX.percent$1(10), tss.usX.px$1(150), false, true, "FileSize", void 0)]), tss.usX.px$1(400))).WithListItems(this.GetDetailsListItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Components in Rows"), tss.UI.TextBlock("DetailsList can host any Tesserae component within its cells."), tss.ICX.MB(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents, [tss.UI.IconColumn(tss.UI.Icon$2(134), tss.usX.px$1(32), false, void 0, void 0), tss.UI.DetailsListColumn("Status", tss.usX.px$1(120), false, false, void 0, void 0), tss.UI.DetailsListColumn("Name", tss.usX.px$1(250), true, false, void 0, void 0), tss.UI.DetailsListColumn("Action", tss.usX.px$1(150), false, false, void 0, void 0), tss.UI.DetailsListColumn("Rating", tss.usX.px$1(400), false, false, void 0, void 0)]).Compact(), tss.usX.px$1(400)).WithListItems(this.GetComponentDetailsListItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Paginated and Empty States"), tss.UI.SplitView().Resizable().SplitInMiddle().Left(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Infinite scrolling")), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1727), tss.usX.px$1(64), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.percent$1(40), true, true, "FileName", void 0), tss.UI.DetailsListColumn$1("Date Modified", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "DateModified", void 0), tss.UI.DetailsListColumn$1("Modified By", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn$1("File Size", tss.usX.percent$1(10), tss.usX.px$1(150), false, true, "FileSize", void 0)]), tss.usX.px$1(300)).WithListItems(this.GetDetailsListItems(0, page)).WithPaginatedItems(H5.fn.bind(this, function () {
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
                }))])).Right(tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.VStack()), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Empty State")), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1727), tss.usX.px$1(64), false, void 0, void 0), tss.UI.DetailsListColumn("File Name", tss.usX.percent$1(40), true, true, "FileName", void 0), tss.UI.DetailsListColumn$1("Date Modified", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "DateModified", void 0), tss.UI.DetailsListColumn$1("Modified By", tss.usX.percent$1(30), tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn$1("File Size", tss.usX.percent$1(10), tss.usX.px$1(150), false, true, "FileSize", void 0)]).WithEmptyMessage(function () {
                    return tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No items found"), tss.usX.px$1(16))))));
                }), tss.usX.px$1(300)).WithListItems(System.Array.init(0, null, Tesserae.Tests.Samples.DetailsListSampleFileItem))]))]));
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
                    return new Tesserae.Tests.Samples.DetailsListSampleFileItem(1765, System.String.format("Document_{0}.docx", [H5.box(n, System.Int32)]), System.DateTime.addDays(System.DateTime.getToday(), ((-n) | 0)), "System", n * 1.5);
                }).ToArray(Tesserae.Tests.Samples.DetailsListSampleFileItem);
            },
            GetComponentDetailsListItems: function () {
                return System.Linq.Enumerable.range(1, 20).select(function (n) {
                    return new Tesserae.Tests.Samples.DetailsListSampleItemWithComponents().WithIcon(1311).WithCheckBox(tss.UI.CheckBox$1("Active").Checked()).WithName(System.String.format("Record {0}", [H5.box(n, System.Int32)])).WithButton(tss.UI.Button$1("Edit").Primary().OnClick(function (s, e) {
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DialogSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Dialogs are modal UI overlays that provide contextual information or require user action, such as confirmation or input. They are designed to capture the user's attention and typically block interaction with the rest of the application until they are dismissed.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Dialogs for critical or short-term tasks that require a decision. Ensure the content is brief and clearly states the purpose. Provide logical action buttons (e.g., 'Confirm' and 'Cancel') and highlight the primary action. Avoid overusing Dialogs for non-essential information to prevent frustrating the user. Consider using non-modal alternatives if the task doesn't require immediate attention.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Dialog").OnClick(function (c, ev) {
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
                }), response]));

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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DropdownSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Dropdown is a list in which the selected item is always visible, and the others are visible on demand by clicking a drop-down button."), tss.UI.TextBlock("They are used to simplify the design and make a choice within the UI. When closed, only the selected item is visible. When users click the drop-down button, all the options become visible.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use a Dropdown when there are multiple choices that can be collapsed under one title, especially if the list of items is long or when space is constrained. Use shortened statements or single words as options. Dropdowns are preferred over radio buttons when the selected option is more important than the alternatives. For less than 7 options, consider using a ChoiceGroup if space allows.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Dropdown"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1("Option 1").Selected(), tss.UI.DropdownItem$1("Option 2"), tss.UI.DropdownItem$1("Option 3")])), tss.UI.Label$1("With Headers and Dividers").SetContent(tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1("Group 1").Header(), tss.UI.DropdownItem$1("Item 1.1"), tss.UI.DropdownItem$1("Item 1.2"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Group 2").Header(), tss.UI.DropdownItem$1("Item 2.1"), tss.UI.DropdownItem$1("Item 2.2").Selected()]))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Selection Modes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Multi-select").SetContent(tss.UI.Dropdown().Multi().Items$1([tss.UI.DropdownItem$1("Apple"), tss.UI.DropdownItem$1("Banana").Selected(), tss.UI.DropdownItem$1("Orange").Selected(), tss.UI.DropdownItem$1("Grape")])), tss.UI.Label$1("Custom Arrow Icon").SetContent(tss.UI.Dropdown().SetArrowIcon(115).Items$1([tss.UI.DropdownItem$1("Low"), tss.UI.DropdownItem$1("Medium").Selected(), tss.UI.DropdownItem$1("High")]))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Async Loading"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Load on open (5s delay)").SetContent(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetItemsAsync))), tss.UI.Label$1("Load immediately (5s delay)").SetContent(Tesserae.Tests.Samples.DropdownSample.StartLoadingAsyncDataImmediately(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetItemsAsync)))), tss.UI.Label$1("Empty State").SetContent(tss.UI.Dropdown$2("No items available").Items$1(System.Array.init(0, null, tss.Dropdown.Item)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Required Dropdown").SetContent(tss.UI.Dropdown().Required().Items$1([tss.UI.DropdownItem$1("Choose one...").Header(), tss.UI.DropdownItem$1("Valid Choice")])), tss.UI.Label$1("Validation (Must select 'Option 1')").SetContent(validatedDropdown)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Dropdowns"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Small").SetContent(tss.ISX.Rounded(tss.Dropdown, tss.UI.Dropdown(), tss.BR.Small).Items$1([tss.UI.DropdownItem$1("Option 1"), tss.UI.DropdownItem$1("Option 2")])), tss.UI.Label$1("Medium").SetContent(tss.ISX.Rounded(tss.Dropdown, tss.UI.Dropdown(), tss.BR.Medium).Items$1([tss.UI.DropdownItem$1("Option 1"), tss.UI.DropdownItem$1("Option 2")])), tss.UI.Label$1("Full").SetContent(tss.ISX.Rounded(tss.Dropdown, tss.UI.Dropdown(), tss.BR.Full).Items$1([tss.UI.DropdownItem$1("Option 1"), tss.UI.DropdownItem$1("Option 2")]))])]));
            }
        },
        methods: {
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("EditableLabelSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("EditableLabels and EditableAreas allow users to view content as standard text and switch to an editing mode (input or textarea) upon interaction."), tss.UI.TextBlock("They are useful for 'in-place' editing where you want to keep the UI clean but allow users to quickly modify specific fields without navigating to a separate form.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use EditableLabels for short, single-line content like titles or names. Use EditableAreas for longer, multi-line content like descriptions. Always provide an OnSave() callback to persist the changes. Ensure the interaction to trigger editing is clear\u2014typically by showing an edit icon on hover or using a distinct visual style. Consider using validation to ensure the entered data meets your requirements.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Editable Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.EditableLabel("Click to edit this text"), tss.ITFX.Bold(tss.EditableLabel, tss.ITFX.Large(tss.EditableLabel, tss.UI.EditableLabel("Large and Bold Title"))), tss.ITFX.MediumPlus(tss.EditableLabel, tss.UI.EditableLabel("Pre-configured font size"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Editable Area"), tss.UI.TextBlock("For multi-line text input:"), tss.ICX.Width(tss.EditableArea, tss.UI.EditableArea("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Click here to edit the entire block of text."), tss.usX.px$1(400)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Events and Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.EditableLabel("Change me and check the toast").OnSave(function (s, text) {
                    tss.UI.Toast().Success(System.String.format("Saved: {0}", [text]));
                    return true;
                }), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required Field")).SetContent(tss.UI.EditableLabel("Can't be empty"))])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("EmojiSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Tesserae includes full support for Emojis via an integrated stylesheet and a strongly-typed enum. This allows you to easily add expressive icons to your application with complete C# IntelliSense support and consistent rendering.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Emojis to add personality and visual cues to your interface. Ensure that the Emojis used are universally understood and appropriate for the context. Avoid using Emojis as the sole way to convey critical information, as their appearance can vary slightly between platforms. Use the SearchableList below to find the exact Emoji you need.")])), tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle(System.String.format("Strongly-typed {0} enum", ["Emoji"])), tss.UI.SearchableList(Tesserae.Tests.Samples.EmojiSample.IconItem, ($t = Tesserae.Tests.Samples.EmojiSample.IconItem, System.Linq.Enumerable.from(this.GetAllIcons(), $t).ToArray($t)), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)])])), true, false, "");
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("FileSelectorAndDropAreaSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("FileSelector and FileDropArea provide two different ways for users to upload files. FileSelector uses a standard button that opens the system file dialog, while FileDropArea provides a larger target area for users to drag and drop files directly into the application.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use FileSelector for simple, single-file selections in forms. Use FileDropArea when users are likely to be uploading multiple files or when a more prominent upload target is desired. Always specify the allowed file types using the 'Accepts' property. Provide immediate feedback after files are selected or dropped, such as displaying the file names or sizes.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("File Selector"), tss.UI.Label$1("Selected file size: ").Inline().SetContent(tss.UI.Var(tss.txt, tss.UI.TextBlock(""), size)), tss.UI.FileSelector().OnFileSelected(function (fs, e) {
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
                }).Multiple()]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("FloatSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Float components are used to place content in absolute-positioned overlays within a relative container. They allow for precise placement of UI elements, such as badges, help icons, or status indicators, without affecting the layout of surrounding components.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Float when you need to position an element independently of the normal document flow. Always ensure the parent container is set to 'Relative' positioning to constrain the floated element. Be careful not to obscure important content or interactive elements beneath the overlay. Use meaningful positions that correlate logically with the parent content.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Possible Positions")), tss.ICTX.Children$6(tss.S, tss.ICX.Height(tss.S, tss.ICX.WS(tss.S, tss.UI.Stack().Relative().Horizontal()), tss.usX.px$1(400)), [tss.UI.Float(tss.UI.Button$1("TopLeft"), "tss-float-topleft"), tss.UI.Float(tss.UI.Button$1("TopMiddle"), "tss-float-topmiddle"), tss.UI.Float(tss.UI.Button$1("TopRight"), "tss-float-topright"), tss.UI.Float(tss.UI.Button$1("LeftCenter"), "tss-float-leftcenter"), tss.UI.Float(tss.UI.Button$1("Center"), "tss-float-center"), tss.UI.Float(tss.UI.Button$1("RightCenter"), "tss-float-rightcenter"), tss.UI.Float(tss.UI.Button$1("BottomLeft"), "tss-float-bottomleft"), tss.UI.Float(tss.UI.Button$1("BottonMiddle"), "tss-float-bottonmiddle"), tss.UI.Float(tss.UI.Button$1("BottomRight"), "tss-float-bottomright")])]));
            }
        },
        methods: {
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

                    var btnReset = tss.UI.Button$1("Reset").SetIcon$1(460).OnClick$1(function () {
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

                    var btnPause = tss.UI.Button$1("Pause").SetIcon$1(3027);
                    btnPause.OnClick$1(function () {
                        isPaused = !isPaused;
                        btnPause.SetIcon$1(isPaused ? 3232 : 3027);
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("GridPickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The GridPicker component provides an interactive grid where each cell can cycle through a predefined number of states. It's highly customizable through its state formatting logic."), tss.UI.TextBlock("Common use cases include scheduling, availability maps, or any scenario where you need to visualize and edit state across two dimensions.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use GridPickers for dense state selection where labels for each cell would be too cluttered. Provide a clear legend or visual cues for what each state represents. Ensure the row and column headers are descriptive. If the grid is large, consider how it will behave on smaller screens. Leverage the state formatting to provide rich feedback, such as changing colors, icons, or text based on the current state.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Simple Schedule Example"), tss.UI.TextBlock("Click on cells to cycle through states: Dead (\u2620), Slow (\ud83d\udc22), and Fast (\ud83d\udc07)."), picker, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Heatmap/Availability Example"), tss.UI.TextBlock("Assigning different background colors based on state levels (0 to 3)."), hourPicker, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dynamic/Calculated Grid"), tss.UI.TextBlock("Using GridPicker for a complex logic visualization (Game of Life)."), Tesserae.Tests.Samples.GridPickerSample.GetGameOfLifeSample()]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("GridSample")), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The Grid component provides a powerful layout system based on CSS Grid. It allows you to define columns, rows, and gaps between items."), tss.UI.TextBlock("Items within a Grid can be explicitly positioned or stretched across multiple tracks, offering full control over complex 2D layouts.")])), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Grid for page-level layouts or complex component structures where both rows and columns need coordination. For simple one-dimensional layouts (horizontal or vertical), consider using HStack or VStack instead. Leverage 'fr' units for flexible columns that fill available space proportionally. Use 'auto-fit' or 'auto-fill' with 'minmax' to create responsive grids that adapt to different screen sizes without media queries.")])), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Fixed and Flexible Columns"), tss.UI.TextBlock("This grid uses two flexible columns (1fr) and one fixed column (200px). The first item is stretched across all columns."), grid, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Responsive Auto-fit Grid"), tss.UI.TextBlock("This grid automatically adjusts the number of columns based on the available width (min 200px per item)."), gridAutoSize]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("HorizontalSeparatorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A HorizontalSeparator visually divides content into groups. It can optionally contain text or other components to label the group it introduces."), tss.UI.TextBlock("The content can be aligned to the left, center, or right of the line.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use separators to provide structure to long forms or pages. Keep labels short and concise. Use them sparingly; too many separators can clutter the UI. Ensure the labels accurately describe the section that follows.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Text Alignment"), tss.UI.HorizontalSeparator("Center Aligned (Default)"), tss.UI.HorizontalSeparator("Left Aligned").Left(), tss.UI.HorizontalSeparator("Right Aligned").Right(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Themed and Custom Content"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.HorizontalSeparator("Primary Color").Primary(), tss.UI.HorizontalSeparator$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.PaddingRight(tss.Icon, tss.UI.Icon$2(2326), tss.usX.px$1(8)), tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Information Section"))])).Primary().Left()]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Empty Separator"), tss.UI.TextBlock("A simple line without any label:"), tss.UI.HorizontalSeparator("")]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("InfiniteScrollingListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("InfiniteScrollingList provides a way to render large sets of items by loading them on demand as the user scrolls. It uses a visibility sensor to detect when the end of the list is reached."), tss.UI.TextBlock("This approach is great for social feeds, search results, or any collection where you want to avoid explicit pagination buttons.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use infinite scrolling for content that is explored discovery-style rather than searched for specifically. Ensure that the loading state is clearly indicated to the user. Consider the performance impact of adding many DOM elements; for extremely large lists, VirtualizedList may be more appropriate. Provide a way for users to reach the footer of the page if necessary, perhaps by offering a 'Load More' button instead of fully automatic scrolling if the footer contains important links.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Vertical Infinite List"), tss.UI.TextBlock("Items are loaded 20 at a time with a small delay to simulate network latency."), tss.ICX.MB(tss.InfiniteScrollingList, tss.ICX.Height(tss.InfiniteScrollingList, tss.UI.InfiniteScrollingList$2(this.GetSomeItems(20, 0, " (Initial Set)"), H5.fn.bind(this, function () {
                    var $tcs = new H5.TCS();
                    (async () => (await H5.toPromise(this.GetSomeItemsAsync(20, H5.identity(page, ((page = (page + 1) | 0)))))))().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                })), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Grid-based Infinite List"), tss.UI.TextBlock("Displaying items in a 3-column grid that expands as you scroll."), tss.ICX.Height(tss.InfiniteScrollingList, tss.UI.InfiniteScrollingList$2(this.GetSomeItems(20, 0, " (Initial Set)"), H5.fn.bind(this, function () {
                    var $tcs = new H5.TCS();
                    (async () => (await H5.toPromise(this.GetSomeItemsAsync(20, H5.identity(pageGrid, ((pageGrid = (pageGrid + 1) | 0)))))))().then(function ($r) { $tcs.sR($r); }, function ($e) { $tcs.sE(System.Exception.create($e)); });
                    return $tcs.task;
                }), [tss.usX.percent$1(33), tss.usX.percent$1(33), tss.usX.percent$1(34)]), tss.usX.px$1(400))]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ItemsListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ItemsList is a versatile component for displaying collections of items. It supports static lists, observable lists, and grid layouts."), tss.UI.TextBlock("It is ideal for smaller to medium-sized collections. For very large datasets, consider using VirtualizedList for better performance.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use ItemsList when you want full control over the rendering of each item. Leverage observable lists to automatically update the UI when the underlying data changes. Use columns to create grid layouts that adapt to the container width. Always provide a meaningful empty message if there are no items to display. If you expect a very high number of items, ensure you test the performance or switch to a virtualized approach.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Simple List"), tss.ICX.MB(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), tss.usX.px$1(250)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Multi-column Grid"), tss.ICX.MB(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(12), [tss.usX.percent$1(33), tss.usX.percent$1(33), tss.usX.percent$1(34)]), tss.usX.px$1(300)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dynamic Observable List"), tss.UI.TextBlock("This list uses a VisibilitySensor to append more items as you scroll."), tss.ICX.MB(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList$1(obsList, [tss.usX.percent$1(50), tss.usX.percent$1(50)]), tss.usX.px$1(300)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Empty State"), tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(System.Array.init(0, null, tss.IC)).WithEmptyMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No items to show"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(150))]));
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

    H5.define("Tesserae.Tests.Samples.LabelSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("LabelSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Labels provide a name or title for a component or a group of components. They are essential for accessibility and helping users understand the purpose of input fields."), tss.UI.TextBlock("While many Tesserae components have built-in labels, the standalone Label component offers more flexibility in positioning and styling.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use sentence casing for label text. Keep labels short and concise, typically using a noun or a short noun phrase. Do not use labels as instructional text; use TextBlocks or tooltips for that purpose. Ensure labels are positioned close to the components they describe. Use the 'Required' flag to clearly indicate mandatory fields.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard Label"), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required Label")), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Label")), tss.txtX.Primary(tss.Label, tss.UI.Label$1("Primary Colored Label"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Label with Content"), tss.UI.Label$1("Username").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your username")), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Inline Layouts"), tss.UI.TextBlock("Labels can be displayed inline with their content, with optional automatic width synchronization."), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Name").Inline().AutoWidth().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Department").Inline().AutoWidth().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Role").Inline().AutoWidth().SetContent(tss.UI.TextBox$1())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Right Aligned Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Short").Inline().AutoWidth(void 0, true).SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("A much longer label").Inline().AutoWidth(void 0, true).SetContent(tss.UI.TextBox$1())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Labels"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.Label, tss.UI.Label$1("Small rounded"), tss.BR.Small), tss.ISX.Rounded(tss.Label, tss.UI.Label$1("Medium rounded"), tss.BR.Medium), tss.ISX.Rounded(tss.Label, tss.UI.Label$1("Full rounded"), tss.BR.Full)])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("LayerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Layer is a technical component used to render content outside of its parent's DOM tree, typically at the end of the document body. This allows content to escape boundaries like 'overflow: hidden' or complex z-index stacks, ensuring that elements like tooltips, context menus, and modals always appear on top of other content.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Layers for UI elements that must appear above all other content regardless of their position in the component hierarchy. Utilize 'LayerHost' when you need to project layered content into a specific part of the DOM instead of the default body location. Be mindful of the lifecycle of layered components to ensure they are properly removed from the DOM when no longer needed.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic layered content")), tss.LayerExtensions.Content(tss.Layer, layer, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("This is example layer content."), tss.UI.Button$1("Show second Layer").SetIcon$1(32).Primary().OnClick(function (s, e) {
                    layer2.IsVisible = true;
                }), tss.LayerExtensions.Content(tss.Layer, layer2, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("This is the second example layer content."), tss.UI.Button$1("Hide second Layer").SetIcon$1(1243).Primary().OnClick(function (s, e) {
                    layer2.IsVisible = false;
                })])), tss.UI.Toggle$1("Toggle Component Layer").OnChange(function (s, e) {
                    layer.IsVisible = s.IsChecked;
                }), tss.UI.Toggle()])), tss.UI.Toggle$1("Toggle Component Layer").OnChange(function (s, e) {
                    layer.IsVisible = s.IsChecked;
                }), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Using LayerHost to control projection")), tss.UI.Toggle$1("Show on Host").OnChange(function (s, e) {
                    layer.Host = s.IsChecked ? layerHost : null;
                }), layerHost]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("MasonrySample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Masonry layout (also known as a Pinterest-style layout) is a grid where items are placed in optimal positions based on available vertical space."), tss.UI.TextBlock("Unlike a standard grid where rows have a uniform height, a Masonry layout allows items of varying heights to be packed tightly together.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Masonry for visually-driven content like image galleries or dashboard widgets with varying heights. Ensure that the number of columns is appropriate for the screen size. Provide a consistent gap between items to maintain a clean appearance. Avoid using Masonry for content that needs to be read in a specific sequential order, as the placement can be non-linear.")])), tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Masonry Grid (4 Columns)"), tss.ScrollBar.ScrollY(tss.Masonry, tss.ICTX.Children$6(tss.Masonry, tss.ICX.S(tss.Masonry, tss.UI.Masonry(4)), ($t = tss.IC, System.Linq.Enumerable.from(this.GetCards(50), $t).ToArray($t))))]), true, false, "");
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("MessageSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The Message component is used to display static messages, alerts, or empty states. It supports an icon, title, text body, and an optional note area."), tss.UI.TextBlock("It comes with variants for standard, success, warning, and error states.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Standard Message (with Note)"), tss.UI.Message("No Database Schema Yet", "Start by describing your database requirements in the chat. I'll help you design a complete schema with tables, relationships, and best practices.").Icon$1(1736).Note$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItems("center"), [tss.ICX.PR(tss.Icon, tss.UI.Icon$2(651, "fi-rr-", "tss-fontsize-small", void 0), 8), tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Try saying: \"Create a blog database with users, posts, and comments\""))])), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Error Message"), tss.UI.Message("Something went wrong", "We couldn't save your changes. Please check your internet connection and try again.").Icon$1(1243).Variant(tss.MessageVariant.Error), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("No Results"), tss.UI.Message("No results found", "We couldn't find any items matching your search criteria.").Icon$1(3605).Variant(tss.MessageVariant.Default)]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ModalSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Modals are large overlays used for tasks that require a separate context, such as creating or editing complex entities, or for displaying rich content that shouldn't clutter the main interface. They provide more space than Dialogs and can host a variety of components.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Modals for multi-step tasks or content-heavy interactions. Ensure that the Modal has a clear title and provide multiple ways to dismiss it (e.g., Close button, clicking outside, or the Escape key). Use 'LightDismiss' for non-critical information and blocking behavior only when user input is essential. Always maintain a clear typographic hierarchy within the Modal content.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Modal").OnClick(function (s, e) {
                    modal.v.Show();
                }), tss.UI.Button$1("Open Modal from top right").OnClick(function (s, e) {
                    modal.v.ShowAt(tss.usX.px$1(16), void 0, tss.usX.px$1(16), void 0);
                }), tss.UI.Button$1("Open Modal with minimum size").OnClick(function (s, e) {
                    tss.ICX.MinWidth(tss.Modal, tss.ICX.MinHeight(tss.Modal, tss.LayerExtensions.Content(tss.Modal, tss.UI.Modal$1().CenterContent().LightDismiss().Dark(), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("small content"))), tss.usX.vh$1(50)), tss.usX.vw$1(50)).Show();
                }), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Embedded Modal"), tss.UI.Button$1("Open Modal Below").OnClick(function (s, e) {
                    container.Content$1(tss.ICX.MinWidth(tss.Modal, tss.ICX.MinHeight(tss.Modal, tss.LayerExtensions.Content(tss.Modal, tss.UI.Modal("Embedded Modal").CenterContent().LightDismiss().Dark(), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("hosted small content"))), tss.usX.vh$1(30)), tss.usX.vw$1(50)).ShowEmbedded());
                }), container]));
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

                navbar.AddHeader(new Tesserae.SidebarButton.$ctor6("brand", 3486, "My App").Primary());
                navbar.AddHeader(new Tesserae.SidebarButton.$ctor6("dashboard", 1300, "Dashboard"));

                navbar.AddContent(new Tesserae.SidebarButton.$ctor6("profile", 4427, "Profile"));
                navbar.AddContent(new Tesserae.SidebarButton.$ctor6("settings", 3641, "Settings"));
                navbar.AddContent(new Tesserae.SidebarSeparator("sep1"));
                navbar.AddContent(new Tesserae.SidebarButton.$ctor6("logout", 3711, "Logout"));

                navbar.AddFooter(new Tesserae.SidebarButton.$ctor6("footer", 2326, "About"));

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("NavbarSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Sidebar rendered as a Navbar. Header items are inline, others are in a drawer."), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.ICX.H$1(tss.S, tss.UI.VStack(), tss.usX.px$1(500)), [navbar, tss.ICX.Padding(tss.txt, tss.UI.TextBlock("Page Content below the navbar..."), tss.usX.px$1(16))])]));
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

                nodeView.DefineNode("Hello World", function (ib) {
                    ib.AddInput("inp", function () {
                        return tss.NodeView.Interfaces.TextInputInterface("Input", "Hi Input");
                    }).AddOutput("out", function () {
                        return tss.NodeView.Interfaces.TextInputInterface("Output", "Hi Output");
                    });
                });

                nodeView.DefineNode("Complex", function (ib) {
                    ib.AddInput("text", function () {
                        return tss.NodeView.Interfaces.TextInterface("Input", "Hi Input");
                    }).AddInput("int", function () {
                        return tss.NodeView.Interfaces.IntegerInterface("Input", 123);
                    }).AddInput("num", function () {
                        return tss.NodeView.Interfaces.NumberInterface("Input", 3.14);
                    }).AddInput("btn", function () {
                        return tss.NodeView.Interfaces.ButtonInterface("Input", function () {
                            tss.UI.Toast().Information("Hi!");
                        });
                    }).AddInput("chk", function () {
                        return tss.NodeView.Interfaces.CheckboxInterface("Input", false);
                    }).AddInput("sel", function () {
                        return tss.NodeView.Interfaces.SelectInterface("Input", "A", System.Array.init(["A", "B", "C"], System.String));
                    }).AddInput("sld", function () {
                        return tss.NodeView.Interfaces.SliderInterface("Input", 0.5, 0, 1);
                    }).AddOutput("out", function () {
                        return tss.NodeView.Interfaces.TextInterface("Output", "Hi Output");
                    });
                });

                nodeView.DefineDynamicNode("Dynamic", function (ib) {
                    ib.AddInput("inp", function () {
                        return tss.NodeView.Interfaces.IntegerInterface("Output Count", 1);
                    });
                }, function (inputState, outputState, ib) {
                    var inputCount = H5.unbox(inputState.inp);
                    for (var i = 0; i < inputCount; i = (i + 1) | 0) {
                        ib.AddOutput(System.String.format("out-{0}", [H5.box(i, System.Int32)]), function () {
                            return tss.NodeView.Interfaces.TextInterface(System.String.format("out-{0}", [H5.box(i, System.Int32)]), H5.toString(i));
                        });
                    }
                });


                var textArea = tss.ICX.Grow(tss.TextArea, tss.ICX.H(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1()), 10));

                nodeView.OnChange(function (v) {
                    textArea.Text = v.GetJsonState(true);
                });

                textArea.OnBlur(function (ta, ev) {
                    nodeView.SetState(ta.Text);
                });

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("NodeViewSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("NodeView is a powerful utility for creating node-based visual editors and data flows. It allows you to define custom node types with various input and output interfaces, enabling users to build complex logic or data pipelines graphically.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use NodeView for scenarios where users need to define relationships or workflows. Keep node definitions logical and consistent. Provide descriptive names for inputs and outputs. Utilize dynamic nodes when the node structure needs to adapt based on its internal state or external data.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.WS(tss.SplitView, tss.ICX.H(tss.SplitView, tss.UI.SplitView().SplitInMiddle().Resizable(), 600)).Left(nodeView).Right(tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [tss.UI.Label$1("JSON State"), textArea]))]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("NumberPickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The NumberPicker provides an input field specifically for numeric values, leveraging the browser's native number input widget."), tss.UI.TextBlock("It supports constraints like minimum and maximum values, as well as step increments for easier value adjustment.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use the NumberPicker whenever a precise numeric input is required. Set appropriate 'min', 'max', and 'step' values to guide the user. If the range of numbers is small and discrete, consider using a Slider or ChoiceGroup instead. Use validation to ensure the entered number meets specific criteria (e.g., must be even or positive).")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic NumberPickers"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.NumberPicker()), tss.UI.Label$1("Initial value (42)").SetContent(tss.UI.NumberPicker(42)), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.NumberPicker().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Constraints"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Between 0 and 100").SetContent(tss.UI.NumberPicker().SetMin(0).SetMax(100)), tss.UI.Label$1("Step increment of 5").SetContent(tss.UI.NumberPicker().SetStep(5))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Must be an even number").SetContent(tss.vX.Validation(tss.NumberPicker, tss.UI.NumberPicker(), function (np) {
                    return np.Value % 2 === 0 ? null : "Please choose an even value";
                })), tss.UI.Label$1("Required field").SetContent(tss.UI.NumberPicker().Required())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.NumberPicker().OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Value changed to: {0}", [H5.box(s.Value, System.Int32)]));
                })]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ObservableStackSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ObservableStack is a specialized container that synchronizes its DOM with an observable list using an efficient reconciliation process."), tss.UI.TextBlock("Instead of re-rendering the entire list when a change occurs, it identifies which elements were added, removed, or moved by comparing their unique Identifiers and ContentHashes. This makes it ideal for high-performance lists where preserving scroll position or component state is important.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use ObservableStack when your list data changes frequently or when you want smooth transitions for moved items. Ensure each item implements 'IComponentWithID' correctly, providing a stable 'Identifier' and a 'ContentHash' that reflects any changes in the item's data. Avoid frequent full-list replacements if only a few items have changed. Leverage the reconciliation behavior to keep the DOM footprint minimal and performance high.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interactive Reconciliation Demo"), tss.UI.TextBlock("Modify the list below and watch how the 'Display Elements' on the right update efficiently."), tss.ICX.WS(tss.SplitView, tss.ICX.Height(tss.SplitView, tss.UI.SplitView(), tss.usX.px$1(500))).Left(tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1("Randomize Order").OnClick$1(function () {
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
                        list.Add(tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignItemsCenter(), [tss.UI.Button$1().SetIcon$1(210).OnClick$1((function ($me, idx) {
                            return H5.fn.bind($me, function () {
                                this.Move(idx.v, ((idx.v - 1) | 0));
                            });
                        })(this, idx)), tss.UI.Button$1().SetIcon$1(170).OnClick$1((function ($me, idx) {
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
                })))]), 8)).Right(tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.SemiBold(tss.Label, tss.UI.Label$1("Rendered Stack:")), tss.ICX.WS(tss.OS, tss.ICX.H$1(tss.OS, obsStack, tss.usX.px$1(450)))]), 8))]));
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

    H5.define("Tesserae.Tests.Samples.OverflowSetSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("OverflowSetSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("OverflowSet is a container that automatically moves items that don't fit into the available space into an overflow menu."), tss.UI.TextBlock("It is commonly used for command bars, navigation menus, or any list of actions where you want to maximize the visibility of primary items while ensuring all items are accessible.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use OverflowSet when you have a horizontal list of items that might exceed the screen width. Order items by importance so that the most critical actions are the last to be moved to the overflow menu. Provide a clear icon or label for the overflow trigger (usually a 'more' icon). Ensure that items in the overflow menu remain fully functional.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic OverflowSet"), tss.UI.TextBlock("Resize the window or container to see items moving into the '...' menu."), tss.ICX.PB(tss.OverflowSet, tss.UI.OverflowSet().Items([tss.UI.Button$1("Action 1").Link().OnClick(function (s, e) {
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
                })]), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("With Icons and Constraints"), tss.ICX.PB(tss.OverflowSet, tss.ICX.MaxWidth(tss.OverflowSet, tss.UI.OverflowSet(), tss.usX.px$1(300)).Items([tss.UI.Button$1("Edit").SetIcon$1(1529).Link(), tss.UI.Button$1("Share").SetIcon$1(3648).Link(), tss.UI.Button$1("Delete").SetIcon$1(4310).Link(), tss.UI.Button$1("Copy").SetIcon$1(1205).Link(), tss.UI.Button$1("Move").SetIcon$1(224).Link()]), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Overflow Index"), tss.UI.TextBlock("Force overflow to start after the first item:"), tss.ICX.MaxWidth(tss.OverflowSet, tss.UI.OverflowSet().SetOverflowIndex(0), tss.usX.px$1(400)).Items([tss.UI.Button$1("Always Visible").Primary(), tss.UI.Button$1("Option A").Link(), tss.UI.Button$1("Option B").Link(), tss.UI.Button$1("Option C").Link()])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PaginationSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Pagination allows users to navigate through a large set of data by breaking it into smaller, manageable chunks called pages."), tss.UI.TextBlock("It provides controls to move between pages, jump to specific pages, and see the current position within the total set.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use pagination when you have a large amount of content that would be overwhelming or slow to load all at once. Clearly show the total number of items and the current page. Provide 'Previous' and 'Next' controls for sequential navigation. If the number of pages is high, consider using a simplified view or allowing the user to jump to the first/last page. Keep the pagination controls in a consistent location, typically at the bottom of the content area.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Pagination"), tss.ICX.MB(tss.Card, tss.UI.Card(status), 16), tss.UI.Pagination(120, 10, 1).OnPageChange(function (p) {
                    status.Text = System.String.format("Showing page {0}", [H5.box(p.CurrentPage, System.Int32)]);
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Small Result Set"), tss.UI.Pagination(25, 10, 1).OnPageChange(function (p) {
                    tss.UI.Toast().Information(System.String.format("Selected page {0}", [H5.box(p.CurrentPage, System.Int32)]));
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Large Result Set"), tss.UI.Pagination(1000, 20, 5).OnPageChange(function (p) {
                    tss.UI.Toast().Information(System.String.format("Selected page {0}", [H5.box(p.CurrentPage, System.Int32)]));
                })]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PanelSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Panels are sliding overlays typically used for creation or management tasks, such as editing a user's profile or configuring settings. They provide a large, temporary surface that slides in from either the left or right side of the screen, keeping the user within the current context while providing space for complex forms or information.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Panels for self-contained tasks that are too large for a Dialog or Modal. Choose the 'Far' side (right) for most common actions, and 'Near' (left) for navigation-related content. Provide clear 'Save' and 'Cancel' actions in the footer. Ensure that the Panel size is appropriate for its content, using wider variants for complex forms. Use 'LightDismiss' to allow users to quickly exit by clicking outside the panel.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open panel").OnClick(function (s, e) {
                    panel.Show();
                })]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Pickers are used to select one or more items, such as people or tags, from a large list. They provide a search-based interface with suggestions."), tss.UI.TextBlock("This component is highly flexible, allowing for custom item rendering, single or multiple selections, and different suggestion behaviors.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Pickers when the number of options is too large for a standard Dropdown. Ensure that the items can be easily searched by text. Use clear icons or visual indicators if it helps users identify the correct item quickly. For multiple selections, consider how the selected items will be displayed\u2014either inline or in a separate list. Provide a helpful 'suggestions title' to guide the user when they interact with the picker.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Multi-selection Picker"), tss.UI.TextBlock("Allows selecting multiple tags from the suggestions."), tss.ICX.MB(tss.Picker(Tesserae.Tests.Samples.PickerSampleItem), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItem, 2147483647, false, 0, true, "Suggested Names").Items(this.GetPickerItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Single Selection Picker"), tss.UI.TextBlock("Limits selection to only one item at a time."), tss.ICX.MB(tss.Picker(Tesserae.Tests.Samples.PickerSampleItem), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItem, 1, false, 0, true, "Select one").Items(this.GetPickerItems()), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Rendered Items"), tss.UI.TextBlock("Using icons and complex components for both suggestions and selections."), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItemWithComponents, 2147483647, false, 0, false, "System Items").Items(this.GetComponentPickerItems())]));
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
                return System.Array.init([new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Bob", 460), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("BOB", 425), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Donuts", 796), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Coffee", 1102), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Chess", 888), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Cooper", 2350)], Tesserae.Tests.Samples.PickerSampleItemWithComponents);
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
                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PivotSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Pivots are used for navigating between different views or categories of content within the same context. They provide a compact way to switch between related data sets, such as different tabs in a settings page or different views of a list.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Pivots to organize content into logical categories. Keep labels short and descriptive. Ensure that the most frequently used views are placed first. Utilize the 'Justified' or 'Centered' styles when the pivot should span the full width of its container. Use the 'Cached' option to preserve the state of a tab's content when switching away.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Normal Style"), this.GetPivot(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Justified Style"), this.GetPivot().Justified(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Centered Style"), this.GetPivot().Centered(), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Cached vs. Not Cached Tabs"), tss.PivotX.Pivot(tss.PivotX.Pivot(tss.UI.Pivot(), "tab1", tss.UI.PivotTitle("Cached"), function () {
                    return tss.ITFX.Regular(tss.txt, tss.UI.TextBlock(System.DateTimeOffset.UtcNow.toString()));
                }, true), "tab2", tss.UI.PivotTitle("Not Cached"), function () {
                    return tss.ITFX.Regular(tss.txt, tss.UI.TextBlock(System.DateTimeOffset.UtcNow.toString()));
                }, false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Cached vs. Not Cached Tabs"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Scroll with limited height"), tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.ICX.MaxHeight(tss.Pivot, tss.UI.Pivot(), tss.usX.px$1(500)), "tab1", tss.UI.PivotTitle("5 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab2", tss.UI.PivotTitle("10 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(20)), 16);
                }), true), "tab3", tss.UI.PivotTitle("50 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(50)), 16);
                }), true), "tab4", tss.UI.PivotTitle("100 Items"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(100)), 16);
                }), true), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Tab Overflow"), tss.ICX.H(tss.SplitView, tss.ICX.WS(tss.SplitView, tss.UI.SplitView().Resizable()), 500).LeftIsSmaller(tss.usX.px$1(300)).Left(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.ICX.S(tss.Pivot, tss.UI.Pivot()), "tab1", tss.UI.PivotTitle("Tab 1"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab2", tss.UI.PivotTitle("Tab 2"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab3", tss.UI.PivotTitle("Tab 3"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab4", tss.UI.PivotTitle("Tab 4"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab5", tss.UI.PivotTitle("Tab 5"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab6", tss.UI.PivotTitle("Tab 6"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab7", tss.UI.PivotTitle("Tab 7"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true), "tab8", tss.UI.PivotTitle("Tab 8"), H5.fn.bind(this, function () {
                    return tss.ICX.PB(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), 16);
                }), true)).Right(tss.txtX.BreakSpaces(tss.txt, tss.ICX.WS(tss.txt, tss.UI.TextBlock("\ud83d\udc48 resize this area to see the overflow adjusting which tabs to show"))))]));
            }
        },
        methods: {
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
                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PivotSelectorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("PivotSelector is a variation of the Pivot component that uses a Dropdown for navigation. It is particularly effective for mobile-first designs or interfaces with a large number of tabs that would otherwise require excessive horizontal scrolling.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use PivotSelector when horizontal space is constrained or when the number of tabs is dynamic and potentially large. Provide clear icons and text for each tab to aid navigation. Utilize the 'SetCommands' feature to surface global actions relevant to all tabs, such as 'Add New' or 'Refresh'.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic PivotSelector"), tss.PivotSelectorX.Pivot$1(tss.PivotSelectorX.Pivot$1(tss.PivotSelectorX.Pivot$1(tss.UI.PivotSelector(), "tab1", "Tab 1", function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 1"), 32));
                }), "tab2", "Tab 2", function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 2"), 32));
                }), "tab3", "Tab 3", function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 3"), 32));
                }), tss.ICX.PT(tss.IC, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("PivotSelector with custom buttons"), 16), tss.PivotSelectorX.Pivot(tss.PivotSelectorX.Pivot(tss.UI.PivotSelector().SetCommands([tss.UI.Button$1().SetIcon$1(32).NoBorder().NoBackground().OnClick$1(function () {
                    alert("Add clicked");
                }), tss.UI.Button$1().SetIcon$1(3641).NoBorder().NoBackground().OnClick$1(function () {
                    alert("Settings clicked");
                })]), "tab1", function () {
                    return tss.UI.Button$1("Tab 1").NoBackground().NoBorder().SetIcon$1(3486);
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 1"), 32));
                }), "tab2", function () {
                    return tss.UI.Button$1("Tab 2").NoBackground().NoBorder().SetIcon$1(749);
                }, function () {
                    return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock("Content for Tab 2"), 32));
                }), tss.ICX.PT(tss.IC, Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("PivotSelector with large number of tabs"), 16), tss.PivotSelectorX.Pivot$2(tss.UI.PivotSelector(), System.Linq.Enumerable.range(1, 20).select(function (i) {
                    return new (System.ValueTuple$3(System.String,System.String,Function)).$ctor1(System.String.format("tab{0}", [H5.box(i, System.Int32)]), System.String.format("Tab {0}", [H5.box(i, System.Int32)]), function () {
                        return tss.UI.Card(tss.ICX.P(tss.txt, tss.UI.TextBlock(System.String.format("Content for Tab {0}", [H5.box(i, System.Int32)])), 32));
                    });
                }).ToArray(System.ValueTuple$3(System.String,System.String,Function)))]));
            }
        },
        methods: {
            Render: function () {
                return this.content.tss$IC$Render();
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ProgressIndicatorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ProgressIndicators provide visual feedback for operations that take more than a few seconds. They show the current completion status and help set expectations for how much work remains. If the total amount of work is unknown, use the indeterminate state or a Spinner instead.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use a ProgressIndicator when the total units to completion can be quantified. Provide a clear label describing the operation in progress. Use the indeterminate state only when the duration is unknown. Combine multiple related steps into a single progress bar for a smoother experience. Avoid letting progress appear to move backwards unless a step failed and is being retried.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("States")), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Empty").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(0), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("30%").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(30), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("60%").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(60), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Full").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(100), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Indeterminate").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Indeterminated(), tss.usX.px$1(400))))]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ProgressModalSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ProgressModal is a specialized modal overlay that combines a title, a message, and a progress indicator. It is used for long-running operations where it is important to block other user interactions until the task is complete, while keeping the user informed of the progress.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use ProgressModal only for operations that truly require the user's focus and shouldn't be interrupted. Ensure that the message provides clear context for what is being processed. Always provide a way to cancel the operation if possible. For background tasks that don't need to block the entire UI, consider using an in-place ProgressIndicator or Spinner instead.")])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Modal").OnClick(function (s, e) {
                    tss.tX.fireAndForget(PlayModal());
                })]));
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
                var saveButton = tss.UI.SaveButton().Pending().OnClick(function () {
                    (async () => {
{ }})()
                });

                var manualButton = tss.UI.SaveButton().NothingToSave();

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SaveButtonSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The SaveButton component is a wrapper around a Button that manages common saving states: Pending, Verifying, Saving, Saved, and Error.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Manual State Control"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [manualButton, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Button$1("Set Nothing to Save").OnClick$1(function () {
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
                })]).Gap(tss.usX.px$1(8))]).Gap(tss.usX.px$1(16)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live Demo"), tss.UI.TextBlock("Click the button below to simulate a save operation."), saveButton.OnClick(function () {
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
                        }})()
                })])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Hover State"), tss.UI.TextBlock("This SaveButton has a hover text configured. Hover over it when it is in Pending state."), tss.UI.SaveButton().Configure("Disabled", void 0, void 0, void 0, void 0, "Enable Now!", 4234, 4235, true).Pending()])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Dynamic Text Update"), tss.UI.TextBlock("This SaveButton text can be updated dynamically."), this.DynamicTextUpdateSample()]));
            }
        },
        methods: {
            DynamicTextUpdateSample: function () {
                var btn = tss.UI.SaveButton().Pending();
                return tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [btn, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Button$1("Update Save Text").OnClick$1(function () {
                    btn.Configure("New Save Text", void 0, void 0, void 0, void 0, void 0, 1407, 1407, true);
                }), tss.UI.Button$1("Update Hover Text").OnClick$1(function () {
                    btn.Configure(void 0, void 0, void 0, void 0, void 0, "New Hover Text", 1407, 1407, true);
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SavingToastSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The SavingToast component helps viewing the state of a saving operation (Saving, Saved, Error) with appropriate icons and colors.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1("Trigger Saving").OnClick$1(function () {
                    tss.UI.SavingToast().Saving("Saving data...");
                }), tss.UI.Button$1("Trigger Saved").OnClick$1(function () {
                    tss.UI.SavingToast().Saved("Data saved successfully!");
                }), tss.UI.Button$1("Trigger Error").OnClick$1(function () {
                    tss.UI.SavingToast().Error("Could not save data.");
                }), tss.UI.Button$1("Trigger Error with Close").OnClick$1(function () {
                    tss.UI.SavingToast().Error("Could not save data.", "Error", true);
                }), tss.UI.Button$1("Many Toasts").OnClickSpinWhile(H5.fn.bind(this, function () {
                    return this.ShowMany();
                }))]).Gap(tss.usX.px$1(8)), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live Demo"), tss.UI.Button$1("Simulate Save Process").OnClick$1(function () {
                    (async () => {
                        {
                            var savingToast = tss.UI.SavingToast();
                            savingToast.Saving("Starting save...");
                            (await H5.toPromise(System.Threading.Tasks.Task.delay(2000)));
                            savingToast.Saved("All done!");
                        }})()
                })]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SearchableGroupedListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("SearchableGroupedList extends the functionality of SearchableList by adding automatic grouping of items based on a 'Group' property."), tss.UI.TextBlock("It provides a structured way to display filtered results, categorized by logical groups like file types, departments, or priority levels.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use SearchableGroupedList when your dataset has a natural hierarchy or categorization that helps users find items faster. Provide a clear header for each group using the header generator. Ensure that the 'IsMatch' logic considers both the item content and the group name if appropriate. Like SearchableList, provide a meaningful 'No Results' message and use additional command slots for relevant actions.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Grouped Search with Custom Headers"), tss.ICX.MB(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(20), function (s) {
                    return tss.UI.HorizontalSeparator$1(tss.ITFX.SemiBold(tss.txt, tss.txtX.Primary(tss.txt, tss.UI.TextBlock(s)))).Left();
                }).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching records"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Grouped Grid Layout"), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(40), function (s) {
                    return tss.ITFX.Bold(tss.Label, tss.txtX.Primary(tss.Label, tss.UI.Label$1(s)));
                }, [tss.usX.percent$1(33), tss.usX.percent$1(33), tss.usX.percent$1(34)]), tss.usX.px$1(500))]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SearchableListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("SearchableList combines a search box with a list of items, providing instant filtering as the user types."), tss.UI.TextBlock("Items must implement the 'ISearchableItem' interface, which defines the matching logic and how each item is rendered.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use SearchableList when you have a moderately sized collection that users need to filter quickly. Ensure the 'IsMatch' implementation is performant and covers all relevant fields. Provide a clear 'No Results' message to help users understand when their search doesn't match anything. Use the 'BeforeSearchBox' and 'AfterSearchBox' slots to add relevant actions like 'Add New' or 'Filter' buttons. For very large datasets, consider server-side filtering or a VirtualizedList.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Searchable List"), tss.ICX.MB(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(10)).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No matching items found"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Searchable Grid with Commands"), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(24), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)]).BeforeSearchBox([tss.UI.Button$1("Filter").SetIcon$1(1776)]).AfterSearchBox([tss.UI.Button$1("Add Item").Primary().SetIcon$1(3250)]), tss.usX.px$1(400))]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SearchBoxSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("SearchBoxes provide an input field for searching through content, allowing users to locate specific items within the website or app."), tss.UI.TextBlock("They include a search icon and a clear button, and support both 'on search' (e.g., when Enter is pressed) and 'search as you type' behaviors.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Always use placeholder text to describe the search scope (e.g., 'Search files'). Use the 'Underlined' style for CommandBars or other minimalist surfaces. Enable 'Search as you type' for small to medium datasets where results can be filtered instantly. Provide a clear visual cue when no results are found. Don't use a SearchBox if you cannot reliably provide accurate results.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic SearchBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Default Search").SetContent(tss.UI.SearchBox("Search...").OnSearch(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Searched for: {0}", [e]));
                })), tss.UI.Label$1("Underlined").SetContent(tss.UI.SearchBox("Search site").Underlined().OnSearch(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Searched for: {0}", [e]));
                })), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.SearchBox("Search disabled").Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Search Behaviors"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Search as you type").SetContent(tss.UI.SearchBox("Type something...").SearchAsYouType().OnSearch(function (s, e) {
                    searchAsYouType.Text = System.String.isNullOrEmpty(e) ? "Waiting for input..." : System.String.format("Current search: {0}", [e]);
                })), searchAsYouType]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Customization"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Custom Icon (Filter)").SetContent(tss.UI.SearchBox("Filter items...").SetIcon(1776)), tss.UI.Label$1("No Icon").SetContent(tss.UI.SearchBox("Iconless search").NoIcon()), tss.UI.Label$1("Fixed Width (250px)").SetContent(tss.ICX.Width(tss.SearchBox, tss.UI.SearchBox("Small search"), tss.usX.px$1(250)))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded SearchBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.SearchBox, tss.UI.SearchBox("Small"), tss.BR.Small), tss.ISX.Rounded(tss.SearchBox, tss.UI.SearchBox("Medium"), tss.BR.Medium), tss.ISX.Rounded(tss.SearchBox, tss.UI.SearchBox("Full"), tss.BR.Full)])]));
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
                var stack = tss.UI.SectionStack();

                this._content = tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SectionStackSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("SectionStack is a high-level layout component designed for creating long-form pages or detailed views. It organizes content into distinct vertical sections, typically with a header and footer, providing a consistent structure for complex information architectures.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use SectionStack for the main content area of your pages. Organize related components into distinct sections to improve readability and scanability. Utilize the 'Title' and 'Commands' features of the SectionStack to provide context and actions at the top of the page.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Dynamic Section Generation"), tss.UI.Label$1("Number of sections:").SetContent(tss.UI.Slider(5, 0, 10, 1).OnInput(H5.fn.bind(this, function (s, e) {
                    this.SetChildren(stack, s.Value);
                })))])), stack]);
                this.SetChildren(stack, 5);
            }
        },
        methods: {
            SetChildren: function (stack, count) {
                stack.Clear();

                for (var i = 0; i < count; i = (i + 1) | 0) {
                    tss.SectionStackX.Section(stack, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ITFX.SemiBold(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock(System.String.format("Section {0}", [H5.box(i, System.Int32)])))), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50))]));
                }
            },
            Render: function () {
                return this._content.tss$IC$Render();
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

                sidebar.AddContent(new Tesserae.SidebarButton.$ctor6("home", 2216, "Home"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor6("profile", 4427, "Profile"));

                sidebar.AddContent(new Tesserae.SidebarSeparator("sep1", "Grouping"));

                var settingsNav = new Tesserae.SidebarNav.$ctor2("settings", 3641, "Settings", true);
                settingsNav.Add(new Tesserae.SidebarButton.$ctor6("general", 3641, "General"));
                settingsNav.Add(new Tesserae.SidebarButton.$ctor6("security", 2581, "Security"));
                settingsNav.Add(new Tesserae.SidebarButton.$ctor6("privacy", 1613, "Privacy"));

                sidebar.AddContent(settingsNav);

                sidebar.AddContent(new Tesserae.SidebarSeparator("sep2"));

                sidebar.AddContent(new Tesserae.SidebarButton.$ctor6("help", 3349, "Help"));


                var lightDark = new Tesserae.SidebarCommand.$ctor3(4036).Tooltip$1("Light Mode");

                lightDark.OnClick(function () {
                    if (tss.UI.Theme.IsDark) {
                        tss.UI.Theme.Light();
                        lightDark.SetIcon$1(4036).Tooltip$1("Light Mode");
                    } else {
                        tss.UI.Theme.Dark();
                        lightDark.SetIcon$1(2808).Tooltip$1("Dark Mode");
                    }
                });

                var toast = new Tesserae.SidebarCommand.$ctor1(450).Tooltip$1("Toast !").OnClick(function () {
                    tss.UI.Toast().Success("Here is your toast \ud83c\udf5e");
                });
                var pizza = new Tesserae.SidebarCommand.$ctor1(468).Tooltip$1("Pizza!").OnClick(function () {
                    tss.UI.Toast().Success("Here is your pizza \ud83c\udf55");
                });
                var cheese = new Tesserae.SidebarCommand.$ctor1(454).Tooltip$1("Cheese !").OnClick(function () {
                    tss.UI.Toast().Success("Here is your cheese \ud83e\uddc0");
                });

                var commands = new Tesserae.SidebarCommands("TOASTS", [lightDark, toast, pizza, cheese]);


                var fireworks = new Tesserae.SidebarCommand.$ctor1(838).Tooltip$1("Confetti !").OnClick(function () {
                    tss.UI.Toast().Success("\ud83c\udf8a");
                });
                var happy = new Tesserae.SidebarCommand.$ctor1(9).Tooltip$1("I like this !").OnClick(function () {
                    tss.UI.Toast().Success("Thanks for your feedback");
                });
                var sad = new Tesserae.SidebarCommand.$ctor1(51).Tooltip$1("I don't like this!").OnClick(function () {
                    tss.UI.Toast().Success("Thanks for your feedback");
                });

                var dotsMenu = new Tesserae.SidebarCommand.$ctor3(2707).OnClickMenu(function () {
                    return System.Array.init([new Tesserae.SidebarButton.$ctor6("MANAGE_ACCOUNT", 4427, "Manage Account"), new Tesserae.SidebarButton.$ctor6("PREFERENCES", 3641, "Preferences"), new Tesserae.SidebarButton.$ctor6("DELETE", 4310, "Delete Account"), new Tesserae.SidebarCommands("EMOTIONS", [new Tesserae.SidebarCommand.$ctor1(9), new Tesserae.SidebarCommand.$ctor1(51), new Tesserae.SidebarCommand.$ctor1(53)]), new Tesserae.SidebarCommands("ADD_DELETE", [new Tesserae.SidebarCommand.$ctor3(3250).Primary(), new Tesserae.SidebarCommand.$ctor3(4310).Danger()]).AlignEnd(), new Tesserae.SidebarButton.$ctor6("SIGNOUT", 3711, "Sign Out")], Tesserae.ISidebarItem);
                });

                var commandsEndAligned = new Tesserae.SidebarCommands("SETTINGS", [fireworks, dotsMenu]).AlignEnd();

                sidebar.AddFooter(new Tesserae.SidebarNav.$ctor1("DEEP_NAV", 350, "Multi-Depth Nav", true).Sortable(true, "trees").AddRange(Tesserae.Tests.Samples.SidebarSample.CreateDeepNav("root")));

                sidebar.AddFooter(new Tesserae.SidebarNav.$ctor1("EMPTY_NAV", 853, "Empty Nav", true).ShowDotIfEmpty().OnOpenIconClick$1(function (e, m) {
                    tss.UI.Toast().Success("You clicked on the icon!");
                }));


                sidebar.AddFooter(commands);
                sidebar.AddFooter(commandsEndAligned);

                sidebar.AddFooter(new Tesserae.SidebarButton.$ctor3("CURIOSITY_REF", new tss.ImageIcon("/assets/img/curiosity-logo.svg"), "By Curiosity", new Tesserae.SidebarBadge.ctor("+3").Foreground(tss.UI.Theme.Primary.Foreground).Background(tss.UI.Theme.Primary.Background), [new Tesserae.SidebarCommand.$ctor3(218).OnClick(function () {
                    window.open("https://github.com/curiosity-ai/tesserae", "_blank");
                })]).Tooltip$1("Made with \u2764 by Curiosity").OnClick(function () {
                    window.open("https://curiosity.ai", "_blank");
                }));


                this._content = tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SidebarSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A fully featured Sidebar with Search, Navigation, Buttons, and Separators."), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.H$1(tss.Sidebar, tss.ICX.S(tss.Sidebar, sidebar), tss.usX.px$1(800))])]));
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
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor6("1", 2216, "Home"));
                sidebar.AddContent(new Tesserae.SidebarSeparator("sep1"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor6("2", 4427, "Profile"));
                sidebar.AddContent(new Tesserae.SidebarSeparator("sep2", "More Options"));
                sidebar.AddContent(new Tesserae.SidebarButton.$ctor6("3", 3641, "Settings"));

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SidebarSeparatorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A separator for the Sidebar component to visually group items."), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.TextBlock("Basic separator:"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.H$1(tss.Sidebar, tss.ICX.S(tss.Sidebar, sidebar), tss.usX.px$1(500))])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SkeletonSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Skeleton loaders are used to provide a placeholder for content that is still loading. They help reduce the perceived load time and prevent layout shifts by reserving the space that the final content will occupy."), tss.UI.TextBlock("They come in various shapes like circles for avatars and rectangles for text or images.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use skeleton loaders when content takes more than a second to load. Match the shape and size of the skeleton as closely as possible to the actual content it replaces. Avoid using skeletons for very fast-loading content as it can cause flickering. Ensure the skeleton's color and animation are subtle and fit with the overall theme.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Avatar and Text Placeholder"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.H(tss.Skeleton, tss.ICX.W(tss.Skeleton, tss.UI.Skeleton(Tesserae.SkeletonType.Circle), 48), 48), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.MB(tss.Skeleton, tss.ICX.ML(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W(tss.Skeleton, tss.UI.Skeleton(), 200), 16), 16), 8), tss.ICX.ML(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W(tss.Skeleton, tss.UI.Skeleton(), 140), 12), 16)])]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Article/Image Placeholder"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.H(tss.Skeleton, tss.ICX.WS(tss.Skeleton, tss.UI.Skeleton(Tesserae.SkeletonType.Rect)), 200), tss.ICX.MT(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.WS(tss.Skeleton, tss.UI.Skeleton()), 16), 16), tss.ICX.MT(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W$1(tss.Skeleton, tss.UI.Skeleton(), tss.usX.percent$1(80)), 16), 8), tss.ICX.MT(tss.Skeleton, tss.ICX.H(tss.Skeleton, tss.ICX.W$1(tss.Skeleton, tss.UI.Skeleton(), tss.usX.percent$1(60)), 16), 8)])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SliderSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Sliders allow users to select a value from a continuous or discrete range of values by moving a thumb along a track."), tss.UI.TextBlock("They are ideal for settings that don't require high precision but benefit from a visual representation of the available range, such as volume or brightness.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use sliders when users need to choose a value from a range where the relative position is more important than the exact value. Provide clear labels for the minimum and maximum values. If the user needs to select a precise number, consider using a NumberPicker alongside or instead of a slider. Use discrete steps (increments) if the available choices are limited to specific intervals.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Sliders"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Continuous Slider (step: 1)").SetContent(s1), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("Current Value: "), tss.UI.DeferSync$3(System.Int32, value, function (v) {
                    return tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(H5.toString(v)));
                })]), tss.UI.Label$1("Discrete Slider (step: 10)").SetContent(s2)]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("States"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Slider")).SetContent(tss.UI.Slider(30, 0, 100, 10).Disabled()), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required Slider")).SetContent(tss.UI.Slider(10, 0, 100, 10))])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SpinnerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Spinners are animated circular indicators used to show that a task is in progress when the exact duration is unknown. They are subtle, lightweight, and can be easily placed inline with content or centered within a container to provide feedback without disrupting the layout.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use a Spinner for tasks that take more than a second but have an indeterminate end time. Include a brief, descriptive label (e.g., 'Loading...', 'Processing...') to give users context. Choose a size that is appropriate for the surrounding content\u2014smaller for inline elements and larger for full-page loading states. Avoid showing multiple spinners simultaneously if possible.")])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner sizes")), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Extra small spinner").SetContent(tss.UI.Spinner().XSmall())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Small spinner").SetContent(tss.UI.Spinner().Small())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Medium spinner").SetContent(tss.UI.Spinner().Medium())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Large spinner").SetContent(tss.UI.Spinner().Large()))])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner label positioning")), tss.UI.Label$1("Spinner with label positioned below").SetContent(tss.UI.Spinner("I am definitely loading...").Below()), tss.UI.Label$1("Spinner with label positioned above").SetContent(tss.UI.Spinner("Seriously, still loading...").Above()), tss.UI.Label$1("Spinner with label positioned to right").SetContent(tss.UI.Spinner("Wait, wait...").Right()), tss.UI.Label$1("Spinner with label positioned to left").SetContent(tss.UI.Spinner("Nope, still loading...").Left())]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SplitViewSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("SplitViews divide a surface into two areas, either horizontally or vertically. They are commonly used for master-detail layouts, navigation sidebars, or resizable workspace areas."), tss.UI.TextBlock("Tesserae provides both 'SplitView' (vertical split) and 'HorizontalSplitView' with support for resizable handles and initial sizing.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use SplitViews when users need to see two related sets of content at the same time. Enable resizability if the ideal balance between the two panels depends on the user's task or screen size. Set sensible minimum and maximum sizes for the panels to prevent them from disappearing or becoming too large. Use distinct background colors or borders to help users distinguish between the two areas.")])), tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Interaction Controls"), tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), [tss.UI.Button$1("Make Non-Resizable").OnClick$1(function () {
                    splitView.NotResizable();
                    horzSplitView.NotResizable();
                    tss.UI.Toast().Information("Resizing disabled");
                }), tss.UI.Button$1("Make Resizable").Primary().OnClick$1(function () {
                    splitView.Resizable();
                    horzSplitView.Resizable();
                    tss.UI.Toast().Information("Resizing enabled");
                })]), 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Vertical SplitView"), tss.ICX.MB(tss.SplitView, splitView, 16), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Horizontal SplitView"), horzSplitView]), true, false, "");
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
                var otherButton = tss.ICX.Fade$2(tss.Button, tss.UI.Button$1().SetIcon$1(4183, tss.UI.Theme.Danger.Background, "tss-fontsize-small", "fi-rr-", false));
                var hoverStack = tss.ICTX.Children$6(tss.S, tss.ICX.MaxWidth(tss.S, tss.UI.HStack(), tss.usX.px$1(500)), [mainButton, otherButton]);

                var sortableStack = tss.ICX.MaxWidth(Tesserae.SortableStack, tss.ICX.PB(Tesserae.SortableStack, tss.ICX.WS(Tesserae.SortableStack, new Tesserae.SortableStack(tss.S.Orientation.Horizontal)).AlignItemsCenter(), 8), tss.usX.px$1(500));
                sortableStack.Add("1", tss.UI.Button$1().SetIcon$1(3));
                sortableStack.Add("2", tss.UI.Button$1().SetIcon$1(4));
                sortableStack.Add("3", tss.UI.Button$1().SetIcon$1(5));
                sortableStack.Add("4", tss.UI.Button$1().SetIcon$1(7));
                sortableStack.Add("5", tss.UI.Button$1().SetIcon$1(9));

                var stack = tss.UI.Stack();
                var countSlider = tss.UI.Slider(5, 0, 10, 1);

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("StackSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Stacks are container components that simplify the use of Flexbox for layout. They allow you to arrange children components either horizontally (HStack) or vertically (VStack)."), tss.UI.TextBlock("Tesserae's Stack also includes advanced features like 'SortableStack' for drag-and-drop reordering.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Stacks as the primary way to organize your UI elements. Use HStack for side-by-side components and VStack for top-to-bottom arrangements. Leverage the 'Gap' property to ensure consistent spacing between children. Use SortableStack when users need to customize the order of items, such as in a dashboard or task list. Avoid deeply nested stacks if a Grid layout would be more appropriate for the complexity.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Live Layout Playground"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.MB(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Number of items:").SetContent(countSlider.OnInput(H5.fn.bind(this, function (s, e) {
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
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Stacks"), tss.UI.TextBlock("Rounding is visible when the stack has a background or border."), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.W(tss.S, tss.ISX.Background(tss.S, tss.ISX.Rounded(tss.S, tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBlock("Small")]), 16), tss.BR.Small), tss.UI.Theme.Colors.Blue200), 100).AlignItemsCenter(), tss.ICX.W(tss.S, tss.ISX.Background(tss.S, tss.ISX.Rounded(tss.S, tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBlock("Medium")]), 16), tss.BR.Medium), tss.UI.Theme.Colors.Blue200), 100).AlignItemsCenter(), tss.ICX.W(tss.S, tss.ISX.Background(tss.S, tss.ISX.Rounded(tss.S, tss.ICX.P(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBlock("Full")]), 16), tss.BR.Full), tss.UI.Theme.Colors.Blue200), 100).AlignItemsCenter()])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("StepperSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Steppers (also known as Wizards) guide users through a multi-step process by breaking it down into smaller, logical chunks."), tss.UI.TextBlock("They manage the visibility of content for each step and provide built-in navigation controls (Previous/Next) while tracking the current progress.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Steppers for complex tasks that have a clear sequential order. Keep each step focused on a single topic to avoid overwhelming the user. Provide clear labels for each step so the user knows what to expect. Use the 'Review' step to allow users to verify their input before the final submission. Ensure that the 'Previous' action allows users to return and modify their entries without losing data.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Registration Wizard"), tss.UI.Stepper([tss.UI.Step("Personal Info", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.MB(tss.txt, tss.UI.TextBlock("Tell us about yourself:"), 16), tss.UI.Label$1("Full Name").SetContent(tss.UI.TextBox$1().SetPlaceholder("John Doe")), tss.UI.Label$1("Email Address").SetContent(tss.UI.TextBox$1().SetPlaceholder("john@example.com"))])), tss.UI.Step("Preferences", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.MB(tss.txt, tss.UI.TextBlock("Customize your experience:"), 16), tss.UI.Toggle$3(tss.UI.TextBlock("Yes"), tss.UI.TextBlock("No")).Checked(), tss.UI.Toggle$3(tss.UI.TextBlock("Dark"), tss.UI.TextBlock("Light")), tss.UI.Label$1("Favorite Color").SetContent(tss.UI.ColorPicker())])), tss.UI.Step("Terms & Review", tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.MB(tss.txt, tss.UI.TextBlock("Please review and accept our terms:"), 16), tss.UI.Card(tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Detailed terms and conditions text goes here..."))), tss.txtX.Required(tss.Label, tss.UI.Label$1("Acceptance")).SetContent(tss.UI.CheckBox$1("I agree to the terms of service")), tss.ICX.MT(tss.Button, tss.UI.Button$1("Complete Registration").Primary(), 16)]))]).OnStepChange(function (s) {
                    tss.UI.Toast().Information(System.String.format("Step {0}: {1}", H5.box(((s.CurrentStepIndex + 1) | 0), System.Int32), s.CurrentStep.Title));
                })]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TextBlockSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TextBlock is the fundamental component for displaying text in Tesserae. It provides a consistent way to apply typography styles, sizes, and weights across your application."), tss.UI.TextBlock("It supports various built-in sizes, from tiny to mega, and different weights and colors.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use the predefined text sizes to maintain visual hierarchy. Use semi-bold or bold weights for headers and important information. Leverage the built-in color options (primary, success, danger, etc.) to convey meaning consistently. For long blocks of text, ensure the width is constrained for better readability. Use 'NoWrap' and text-overflow properties when dealing with limited space, such as in list items.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Text Sizes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Mega(tss.txt, tss.UI.TextBlock("Mega Text")), tss.ITFX.XXLarge(tss.txt, tss.UI.TextBlock("XXLarge Text")), tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock("XLarge Text")), tss.ITFX.Large(tss.txt, tss.UI.TextBlock("Large Text")), tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock("MediumPlus Text")), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Medium Text (Default)")), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("SmallPlus Text")), tss.ITFX.Small(tss.txt, tss.UI.TextBlock("Small Text")), tss.ITFX.XSmall(tss.txt, tss.UI.TextBlock("XSmall Text")), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("Tiny Text"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Weights and Colors"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Primary(tss.txt, tss.ITFX.Bold(tss.txt, tss.UI.TextBlock("Bold Primary Text"))), tss.txtX.Success(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Semi-Bold Success Text"))), tss.txtX.Danger(tss.txt, tss.ITFX.Regular(tss.txt, tss.UI.TextBlock("Regular Danger Text")))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Wrapping and Overflow"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Default wrapping:")), tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."), tss.usX.px$1(300)), tss.ICX.MT(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("No wrapping (ellipsis):")), 16), tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("This is a very long text that will be truncated with an ellipsis because it has NoWrap set and a constrained width.")), tss.usX.px$1(300))])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TextBoxSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TextBoxes allow users to enter and edit text. They are used in forms, search queries, and anywhere text input is required."), tss.UI.TextBlock("They support various modes like password input, read-only states, and built-in validation.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Always label your TextBoxes so users know what information is expected. Use placeholder text to provide a hint about the format or content. Mark required fields clearly. Use validation to provide immediate feedback on the correctness of the input. Use the appropriate input type (e.g., Password) for sensitive information. Provide a clear way to submit or clear the data.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic TextBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Standard").SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Placeholder").SetContent(tss.UI.TextBox$1().SetPlaceholder("Enter your name...")), tss.UI.Label$1("Password").SetContent(tss.UI.TextBox$1().Password()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.TextBox$1("Disabled content").Disabled()), tss.UI.Label$1("Read-only").SetContent(tss.UI.TextBox$1("Read-only content").ReadOnly())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Validation"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Must not be empty").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1(), function (tb) {
                    return System.String.isNullOrWhiteSpace(tb.Text) ? "This field is required" : null;
                })), tss.UI.Label$1("Positive Integer only").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1(), tss.Validation.NonZeroPositiveInteger)), tss.UI.Label$1("Custom Error").SetContent(tss.ICVX.IsInvalid(tss.TextBox, tss.ICVX.Error(tss.TextBox, tss.UI.TextBox$1(), "Something went wrong")))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.TextBox$1().SetPlaceholder("Type and check toast...").OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Text changed to: {0}", [s.Text]));
                }), tss.UI.TextBox$1().SetPlaceholder("Search-like behavior...").OnInput(function (s, e) {
                    console.log(System.String.format("Current input: {0}", [s.Text]));
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded TextBoxes"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ISX.Rounded(tss.TextBox, tss.UI.TextBox$1().SetPlaceholder("Small"), tss.BR.Small), tss.ISX.Rounded(tss.TextBox, tss.UI.TextBox$1().SetPlaceholder("Medium"), tss.BR.Medium), tss.ISX.Rounded(tss.TextBox, tss.UI.TextBox$1().SetPlaceholder("Full"), tss.BR.Full)])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TextBreadcrumbsSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TextBreadcrumbs are a navigational aid that indicates the current position within a hierarchy. They allow users to understand their context and easily navigate back to higher-level pages."), tss.UI.TextBlock("This component is typically placed at the top of a page, below the main navigation.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use breadcrumbs for applications with deep hierarchical structures. Place them consistently at the top of the content area. Use short, descriptive labels for each level. The last item in the breadcrumb should represent the current page and is typically not clickable. Breadcrumbs should complement, not replace, the primary navigation system.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Breadcrumbs"), tss.ICX.PB(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs().Items([tss.UI.TextBreadcrumb("Home").OnClick(function (s, e) {
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
                }), tss.UI.TextBreadcrumb("Current File")]), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Long Breadcrumb List"), tss.UI.TextBreadcrumbs().Items([tss.UI.TextBreadcrumb("Resources"), tss.UI.TextBreadcrumb("Images"), tss.UI.TextBreadcrumb("Icons"), tss.UI.TextBreadcrumb("UIcons"), tss.UI.TextBreadcrumb("Regular"), tss.UI.TextBreadcrumb("Arrows"), tss.UI.TextBreadcrumb("Chevron-Down.png")])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ThemeColorsSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ThemeColors allows for real-time inspection and customization of the application's theme. It provides a detailed view of the primary, secondary, and semantic colors used throughout the UI, and allows you to experiment with different primary and background color combinations for both light and dark modes.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use this sample to verify the accessibility and contrast of your theme choices. Ensure that primary and background colors provide sufficient contrast for readability in both light and dark themes. Changes made here are applied immediately to the entire application, allowing for rapid prototyping of different brand identities.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem), tss.UI.DetailsList(Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem, [tss.UI.DetailsListColumn("ThemeName", tss.usX.px$1(120), false, false, void 0, void 0), tss.UI.DetailsListColumn("Background", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("Foreground", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("Border", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("BackgroundActive", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("BackgroundHover", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("ForegroundActive", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("ForegroundHover", tss.usX.px$1(160), false, false, void 0, void 0)]).Compact(), tss.usX.px$1(500)).WithListItems(System.Array.init([new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Default"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Primary"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Secondary"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Success"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Danger")], Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem)).SortedBy("Name"), tss.UI.Label$1("Primary Light").Inline().SetContent(cpPrimaryLight), tss.UI.Label$1("Primary Dark").Inline().SetContent(cpPrimaryDark), tss.UI.Label$1("Background Light").Inline().SetContent(cpBackgroundLight), tss.UI.Label$1("Background Dark").Inline().SetContent(cpBackgroundDark)]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TimelineSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Timeline displays a series of events in chronological order, using a vertical line to connect them."), tss.UI.TextBlock("It is ideal for activity feeds, version histories, or any process where the sequence of steps is important.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Timelines to show the progression of time or a sequence of related events. Clearly distinguish between past, current, and future events if applicable. Use the 'SameSide' property if you want a more linear, left-aligned layout, or the default staggered layout for a more balanced visual look. Ensure that each event has a clear timestamp and a concise description.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Default Staggered Timeline"), tss.ICX.MB(tss.Timeline, tss.ICX.Height(tss.Timeline, tss.ICTX.Children$6(tss.Timeline, tss.UI.Timeline(), this.GetSomeItems(6)), tss.usX.px$1(300)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Same Side Alignment"), tss.ICX.MB(tss.Timeline, tss.ICX.Height(tss.Timeline, tss.ICTX.Children$6(tss.Timeline, tss.UI.Timeline().SameSide(), this.GetSomeItems(6)), tss.usX.px$1(300)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Constrained Width"), tss.ICX.Height(tss.Timeline, tss.ICTX.Children$6(tss.Timeline, tss.UI.Timeline().TimelineWidth(tss.usX.px$1(400)), this.GetSomeItems(6)), tss.usX.px$1(300))]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetSomeItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (n) {
                    return tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(System.String.format("Event {0}", [H5.box(n, System.Int32)]))), tss.ITFX.Small(tss.txt, tss.UI.TextBlock(System.String.format("{0:t} - Description of the event happens here.", [H5.box(System.DateTime.addHours(System.DateTime.getToday(), ((-n) | 0)), System.DateTime, System.DateTime.format)])))]);
                }).ToArray(tss.S);
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
                var stack = tss.UI.SectionStack();
                var countSlider = tss.UI.Slider(5, 0, 10, 1);

                var size = new (tss.SettableObservableT(System.Int32))();
                var deferedWithChangingSize = tss.ICX.WS(tss.IDefer, tss.UI.DeferSync$3(System.Int32, size, function (sz) {
                    return tss.ICX.H(tss.Button, tss.UI.Button$1(System.String.format("Height = {0:n0}px", [H5.box(sz, System.Int32)])), sz);
                }));
                var _discard1 = { };

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TippySample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Tippy is the underlying engine for tooltips and popovers in Tesserae. It provides a flexible way to attach rich, interactive content to any component, with support for various animations, placements, and trigger events.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use tooltips to provide additional context or information without cluttering the main UI. Keep text tooltips brief and focused. For interactive tooltips, ensure the content is easy to use and provides clear affordances. Utilize animations sparingly to enhance the user experience without being distracting. Always consider the placement of the tooltip to ensure it doesn't obscure relevant content.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.Tooltip$1(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Hover me"), 200), "This is a simple text tooltip"), tss.ICX.Tooltip$1(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Animated Tooltip"), 200), "This is a simple text tooltip with animations", "shift-away-subtle"), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Interactive Tooltip"), 200), tss.UI.Button$1("Click me").OnClick$1(function () {
                    tss.UI.Toast().Success("You clicked!");
                }), true, "none", "top", 250, 0, true, false, 350, true, false, void 0, void 0), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Defers on Tooltips"), 200), deferedWithChangingSize), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Nested Tooltips"), 200), tss.UI.Button$1("Click me").OnClick(function (b1, _) {
                    tss.tippy.ShowFor$1(b1, tss.UI.Button$1("Click me").OnClick$1(function () {
                        tss.UI.Toast().Success("You clicked!");
                    }), _discard1);
                }), true, "none", "top", 250, 0, true, false, 350, true, false, void 0, void 0)])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ToastSample")), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Toasts are short-lived, non-intrusive notifications that provide feedback about an operation. They appear temporarily on the screen and then disappear automatically, making them ideal for success messages, warnings, or simple information updates.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Toasts for brief, informative messages that don't require user action. Keep the text short and recognizable. Ensure the Toast duration is long enough to be read but short enough not to become an annoyance. Avoid overloading the user with too many simultaneous Toasts. For critical errors that require immediate attention or user interaction, use a Dialog or Modal instead.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top-right (default)"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick$1(function () {
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
                })])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ToggleSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Toggle represents a physical switch that allows users to choose between two mutually exclusive options, typically 'on' and 'off'."), tss.UI.TextBlock("Unlike a Checkbox, a Toggle is intended for immediate actions where the change takes effect as soon as the switch is flipped.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use Toggles for binary settings that have an immediate effect (e.g., turning Wi-Fi on or off). Labels should be short and describe the setting clearly. Avoid using Toggles when a user needs to click a 'Submit' or 'Apply' button to save changes; use a Checkbox instead. Ensure that the 'on' and 'off' states are visually distinct and easy to understand at a glance.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Basic Toggles"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Default (Unchecked)").SetContent(tss.UI.Toggle()), tss.UI.Label$1("Checked").SetContent(tss.UI.Toggle().Checked()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Checked")).SetContent(tss.UI.Toggle().Checked().Disabled()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled Unchecked")).SetContent(tss.UI.Toggle().Disabled())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Custom Labels and Inline"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Toggle().SetText("With Label"), tss.UI.Toggle$3(tss.UI.TextBlock("Visible"), tss.UI.TextBlock("Hidden")), tss.UI.Label$1("Inline Toggle").Inline().SetContent(tss.UI.Toggle())]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Event Handling"), tss.UI.Toggle().SetText("Feature X").OnChange(function (s, e) {
                    tss.UI.Toast().Information(System.String.format("Feature X is now {0}", [(s.IsChecked ? "Enabled" : "Disabled")]));
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Formatting"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ITFX.Tiny(tss.Toggle, tss.UI.Toggle$1("Tiny")), tss.ITFX.Small(tss.Toggle, tss.UI.Toggle$1("Small (default)")), tss.ITFX.SmallPlus(tss.Toggle, tss.UI.Toggle$1("Small Plus")), tss.ITFX.Medium(tss.Toggle, tss.UI.Toggle$1("Medium")), tss.ITFX.Large(tss.Toggle, tss.UI.Toggle$1("Large")), tss.ITFX.XLarge(tss.Toggle, tss.UI.Toggle$1("XLarge")), tss.ITFX.XXLarge(tss.Toggle, tss.UI.Toggle$1("XXLarge")), tss.ITFX.Mega(tss.Toggle, tss.UI.Toggle$1("Mega")), tss.ITFX.Bold(tss.Toggle, tss.UI.Toggle$1("Bold text"))]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Rounded Toggles"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Label$1("Small").SetContent(tss.ISX.Rounded(tss.Toggle, tss.UI.Toggle(), tss.BR.Small)), tss.UI.Label$1("Medium").SetContent(tss.ISX.Rounded(tss.Toggle, tss.UI.Toggle(), tss.BR.Medium)), tss.UI.Label$1("Full").SetContent(tss.ISX.Rounded(tss.Toggle, tss.UI.Toggle(), tss.BR.Full))])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TutorialModalSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TutorialModal is a specialized modal designed for guided processes, such as onboarding or feature walkthroughs. It combines a large content area with a dedicated help panel and an optional illustrative image, providing a structured environment for users to learn while they interact.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use TutorialModals for complex tasks that benefit from additional explanation and guidance. Ensure that the help text is clear and directly relates to the fields in the content area. Use images or icons to provide visual cues. Always provide a clear way for users to complete or discard the process. Avoid overwhelming users with too much information; keep both the content and the help text concise.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Tutorial Modal").OnClick(function (s, e) {
                    Tesserae.Tests.Samples.TutorialModalSample.SampleTutorialModal().Show();
                }), tss.UI.Button$1("Open Large Tutorial Modal").OnClick(function (s, e) {
                    Tesserae.Tests.Samples.TutorialModalSample.SampleTutorialModal().Height(tss.usX.vh$1(90)).Width(tss.usX.vw$1(90)).Show();
                }), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Embedded Modal"), tss.UI.Button$1("Open Modal Below").OnClick(function (s, e) {
                    container.Content$1(Tesserae.Tests.Samples.TutorialModalSample.SampleTutorialModal().Border("#ffaf66", tss.usX.px$1(5)).ShowEmbedded());
                }), container]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("UIconsSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("UIcons provide a massive collection of high-quality icons integrated directly into Tesserae. They are accessible through a strongly-typed enum, offering full IntelliSense support and ensuring that your application's iconography is consistent and easily maintainable.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use icons to provide visual context and improve the scanability of your UI. Choose icons that are widely recognized and relevant to the action or content they represent. Maintain consistency in icon style and weight throughout your application. Use the SearchableList below to explore the thousands of available icons.")])), tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle(System.String.format("Strongly-typed {0} enum", ["UIcons"])), tss.UI.SearchableList(Tesserae.Tests.Samples.UIconsSample.IconItem, ($t = Tesserae.Tests.Samples.UIconsSample.IconItem, System.Linq.Enumerable.from(this.GetAllIcons(), $t).ToArray($t)), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)])])), true, false, "");
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

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ValidatorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Validator is a utility component that aggregates the validation state of multiple UI components. It provides a centralized way to monitor whether a form or a set of inputs is valid, making it easy to provide real-time feedback to users and prevent the submission of incorrect data.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Register all related input components with a single Validator. Use clear and descriptive validation messages that help users correct errors. Avoid showing validation errors immediately on form load; instead, allow users to interact with the fields first. Use the Validator's state to enable or disable primary actions like 'Submit' or 'Save' to ensure only valid data is processed.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic TextBox")), tss.ICTX.Children$6(tss.S, tss.ICX.Padding(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), tss.usX.px$1(8)), [tss.UI.Label$1("Non-empty").SetContent(textBoxThatMustBeNonEmpty), tss.UI.Label$1("Integer > 0 (must not match the value above)").SetContent(textBoxThatMustBePositiveInteger), tss.UI.Label$1("Pre-filled Integer > 0 (initially valid)").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("123").Required(), tss.Validation.NonZeroPositiveInteger, validator)), tss.UI.Label$1("Pre-filled Integer > 0 (initially i  nvalid)").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("xyz").Required(), tss.Validation.NonZeroPositiveInteger, validator)), tss.UI.Label$1("Not empty with forced instant validation").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("").Required(), function (tb) {
                    return System.String.isNullOrWhiteSpace(tb.Text) ? "Can't be empty" : null;
                }, validator, true)), tss.UI.Label$1("Please select something").SetContent(dropdown)]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Results Summary")), tss.ICTX.Children$6(tss.S, tss.ICX.Padding(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), tss.usX.px$1(8)), [tss.UI.Label$1("Validity (this only checks fields that User has interacted with so far)").Inline().SetContent(looksValidSoFar), tss.UI.Label$1("Test revalidation (will fail if repeated)").SetContent(tss.UI.Button$1("Validate").OnClick(function (s, b) {
                    validator.Revalidate();
                }))])]));

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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("VirtualizedListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("VirtualizedList is a high-performance component designed for rendering massive datasets\u2014thousands or even tens of thousands of items\u2014without sacrificing UI responsiveness."), tss.UI.TextBlock("It achieves this by only rendering the items that are currently visible within the viewport (plus a small buffer), significantly reducing the number of DOM elements the browser needs to manage.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.TextBlock("Use VirtualizedList for any list that could potentially contain more than a few hundred items. Ensure that each item has a consistent height for accurate scroll position calculation. Virtualization is most effective when item components are relatively complex or resource-intensive to render. Always provide a clear 'Empty Message' if the dataset is expected to be empty.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Virtualized List with 5,000 Items"), tss.UI.TextBlock("Scroll rapidly to see how the list handles a large number of items."), tss.ICX.MB(tss.VirtualizedList, tss.ICX.Height(tss.VirtualizedList, tss.UI.VirtualizedList().WithListItems(this.GetALotOfItems()), tss.usX.px$1(400)), 32), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Empty State"), tss.ICX.Height(tss.VirtualizedList, tss.UI.VirtualizedList().WithEmptyMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HS(tss.BackgroundArea, tss.ICX.WS(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.UI.TextBlock("No items available"))))), tss.usX.px$1(100));
                }).WithListItems(System.Linq.Enumerable.empty()), tss.usX.px$1(150))]));
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

    var $m = H5.setMetadata,
        $n = ["System","Tesserae","System.Threading.Tasks","Tesserae.Tests.Samples","System.Collections.Generic"];
    $m("Tesserae.Tests.App", function () { return {"att":1048960,"a":4,"s":true,"m":[{"a":1,"n":"CenteredCardWithBackground","is":true,"t":8,"pi":[{"n":"content","pt":tss.IC,"ps":0}],"sn":"CenteredCardWithBackground","rt":tss.BackgroundArea,"p":[tss.IC]},{"a":1,"n":"FormatSampleName","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].Type,"ps":0}],"sn":"FormatSampleName","rt":$n[0].String,"p":[$n[0].Type]},{"a":1,"n":"Main","is":true,"t":8,"sn":"Main","rt":$n[0].Void},{"a":1,"n":"_sidebarOpenStateKey","is":true,"t":4,"rt":$n[0].String,"sn":"_sidebarOpenStateKey"},{"a":1,"n":"_sidebarOrderKey","is":true,"t":4,"rt":$n[0].String,"sn":"_sidebarOrderKey"}]}; }, $n);
    $m("Tesserae.Tests.SamplesSourceCode", function () { return {"att":1048576,"a":4,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"GetCodeForSample","is":true,"t":8,"pi":[{"n":"sampleName","pt":$n[0].String,"ps":0}],"sn":"GetCodeForSample","rt":$n[0].String,"p":[$n[0].String]}]}; }, $n);
    $m("Tesserae.Tests.ISample", function () { return {"att":1048737,"a":2}; }, $n);
    $m("Tesserae.Tests.Sample", function () { return {"att":1048576,"a":4,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String,$n[0].String,$n[0].Int32,$n[1].UIcons,Function],"pi":[{"n":"type","pt":$n[0].String,"ps":0},{"n":"name","pt":$n[0].String,"ps":1},{"n":"group","pt":$n[0].String,"ps":2},{"n":"order","pt":$n[0].Int32,"ps":3},{"n":"icon","pt":$n[1].UIcons,"ps":4},{"n":"contentGenerator","pt":Function,"ps":5}],"sn":"ctor"},{"a":2,"n":"ContentGenerator","t":16,"rt":Function,"g":{"a":2,"n":"get_ContentGenerator","t":8,"rt":Function,"fg":"ContentGenerator"},"fn":"ContentGenerator"},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"fn":"Group"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"fn":"Icon"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":2,"n":"Order","t":16,"rt":$n[0].Int32,"g":{"a":2,"n":"get_Order","t":8,"rt":$n[0].Int32,"fg":"Order","box":function ($v) { return H5.box($v, System.Int32);}},"fn":"Order"},{"a":2,"n":"Type","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Type","t":8,"rt":$n[0].String,"fg":"Type"},"fn":"Type"},{"a":1,"backing":true,"n":"<ContentGenerator>k__BackingField","t":4,"rt":Function,"sn":"ContentGenerator"},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"},{"a":1,"backing":true,"n":"<Order>k__BackingField","t":4,"rt":$n[0].Int32,"sn":"Order","box":function ($v) { return H5.box($v, System.Int32);}},{"a":1,"backing":true,"n":"<Type>k__BackingField","t":4,"rt":$n[0].String,"sn":"Type"}]}; }, $n);
    $m("Tesserae.Tests.SampleDetailsAttribute", function () { return {"att":1048577,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"s":{"a":2,"n":"set_Group","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Group"},"fn":"Group"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"s":{"a":2,"n":"set_Icon","t":8,"p":[$n[1].UIcons],"rt":$n[0].Void,"fs":"Icon"},"fn":"Icon"},{"a":2,"n":"Order","t":16,"rt":$n[0].Int32,"g":{"a":2,"n":"get_Order","t":8,"rt":$n[0].Int32,"fg":"Order","box":function ($v) { return H5.box($v, System.Int32);}},"s":{"a":2,"n":"set_Order","t":8,"p":[$n[0].Int32],"rt":$n[0].Void,"fs":"Order"},"fn":"Order"},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Order>k__BackingField","t":4,"rt":$n[0].Int32,"sn":"Order","box":function ($v) { return H5.box($v, System.Int32);}}]}; }, $n);
    $m("Tesserae.Tests.Samples.BreadcrumbSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2707
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 4088
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetComponentDetailsListItems","t":8,"sn":"GetComponentDetailsListItems","rt":System.Array.type(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents)},{"a":1,"n":"GetDetailsListItems","t":8,"pi":[{"n":"start","dv":1,"o":true,"pt":$n[0].Int32,"ps":0},{"n":"count","dv":100,"o":true,"pt":$n[0].Int32,"ps":1}],"sn":"GetDetailsListItems","rt":System.Array.type(Tesserae.Tests.Samples.DetailsListSampleFileItem),"p":[$n[0].Int32,$n[0].Int32]},{"a":1,"n":"GetDetailsListItemsAsync","t":8,"pi":[{"n":"start","dv":1,"o":true,"pt":$n[0].Int32,"ps":0},{"n":"count","dv":100,"o":true,"pt":$n[0].Int32,"ps":1}],"sn":"GetDetailsListItemsAsync","rt":$n[2].Task$1(System.Array.type(Tesserae.Tests.Samples.DetailsListSampleFileItem)),"p":[$n[0].Int32,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSampleFileItem", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].UIcons,$n[0].String,$n[0].DateTime,$n[0].String,$n[0].Double],"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0},{"n":"name","pt":$n[0].String,"ps":1},{"n":"modified","pt":$n[0].DateTime,"ps":2},{"n":"by","pt":$n[0].String,"ps":3},{"n":"size","pt":$n[0].Double,"ps":4}],"sn":"ctor"},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[3].DetailsListSampleFileItem,"ps":0},{"n":"key","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[3].DetailsListSampleFileItem,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"index","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[4].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"cell","pt":Function,"ps":1}],"sn":"Render","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[4].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"DateModified","t":16,"rt":$n[0].DateTime,"g":{"a":2,"n":"get_DateModified","t":8,"rt":$n[0].DateTime,"fg":"DateModified","box":function ($v) { return H5.box($v, System.DateTime, System.DateTime.format);}},"fn":"DateModified"},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"FileIcon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_FileIcon","t":8,"rt":$n[1].UIcons,"fg":"FileIcon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"fn":"FileIcon"},{"a":2,"n":"FileName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_FileName","t":8,"rt":$n[0].String,"fg":"FileName"},"fn":"FileName"},{"a":2,"n":"FileSize","t":16,"rt":$n[0].Double,"g":{"a":2,"n":"get_FileSize","t":8,"rt":$n[0].Double,"fg":"FileSize","box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},"fn":"FileSize"},{"a":2,"n":"ModifiedBy","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ModifiedBy","t":8,"rt":$n[0].String,"fg":"ModifiedBy"},"fn":"ModifiedBy"},{"a":1,"backing":true,"n":"<DateModified>k__BackingField","t":4,"rt":$n[0].DateTime,"sn":"DateModified","box":function ($v) { return H5.box($v, System.DateTime, System.DateTime.format);}},{"a":1,"backing":true,"n":"<FileIcon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"FileIcon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<FileName>k__BackingField","t":4,"rt":$n[0].String,"sn":"FileName"},{"a":1,"backing":true,"n":"<FileSize>k__BackingField","t":4,"rt":$n[0].Double,"sn":"FileSize","box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},{"a":1,"backing":true,"n":"<ModifiedBy>k__BackingField","t":4,"rt":$n[0].String,"sn":"ModifiedBy"}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSampleItemWithComponents", function () { return {"att":1048577,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[3].DetailsListSampleItemWithComponents,"ps":0},{"n":"key","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[3].DetailsListSampleItemWithComponents,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"index","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[4].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"cell","pt":Function,"ps":1}],"sn":"Render","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[4].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"WithButton","t":8,"pi":[{"n":"btn","pt":tss.Button,"ps":0}],"sn":"WithButton","rt":$n[3].DetailsListSampleItemWithComponents,"p":[tss.Button]},{"a":2,"n":"WithCheckBox","t":8,"pi":[{"n":"cb","pt":tss.ChecBox,"ps":0}],"sn":"WithCheckBox","rt":$n[3].DetailsListSampleItemWithComponents,"p":[tss.ChecBox]},{"a":2,"n":"WithChoiceGroup","t":8,"pi":[{"n":"cg","pt":tss.ChoiceGroup,"ps":0}],"sn":"WithChoiceGroup","rt":$n[3].DetailsListSampleItemWithComponents,"p":[tss.ChoiceGroup]},{"a":2,"n":"WithIcon","t":8,"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0}],"sn":"WithIcon","rt":$n[3].DetailsListSampleItemWithComponents,"p":[$n[1].UIcons]},{"a":2,"n":"WithName","t":8,"pi":[{"n":"name","pt":$n[0].String,"ps":0}],"sn":"WithName","rt":$n[3].DetailsListSampleItemWithComponents,"p":[$n[0].String]},{"a":2,"n":"Button","t":16,"rt":tss.Button,"g":{"a":2,"n":"get_Button","t":8,"rt":tss.Button,"fg":"Button"},"s":{"a":1,"n":"set_Button","t":8,"p":[tss.Button],"rt":$n[0].Void,"fs":"Button"},"fn":"Button"},{"a":2,"n":"CheckBox","t":16,"rt":tss.ChecBox,"g":{"a":2,"n":"get_CheckBox","t":8,"rt":tss.ChecBox,"fg":"CheckBox"},"s":{"a":1,"n":"set_CheckBox","t":8,"p":[tss.ChecBox],"rt":$n[0].Void,"fs":"CheckBox"},"fn":"CheckBox"},{"a":2,"n":"ChoiceGroup","t":16,"rt":tss.ChoiceGroup,"g":{"a":2,"n":"get_ChoiceGroup","t":8,"rt":tss.ChoiceGroup,"fg":"ChoiceGroup"},"s":{"a":1,"n":"set_ChoiceGroup","t":8,"p":[tss.ChoiceGroup],"rt":$n[0].Void,"fs":"ChoiceGroup"},"fn":"ChoiceGroup"},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"s":{"a":1,"n":"set_Icon","t":8,"p":[$n[1].UIcons],"rt":$n[0].Void,"fs":"Icon"},"fn":"Icon"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"s":{"a":1,"n":"set_Name","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Name"},"fn":"Name"},{"a":1,"backing":true,"n":"<Button>k__BackingField","t":4,"rt":tss.Button,"sn":"Button"},{"a":1,"backing":true,"n":"<CheckBox>k__BackingField","t":4,"rt":tss.ChecBox,"sn":"CheckBox"},{"a":1,"backing":true,"n":"<ChoiceGroup>k__BackingField","t":4,"rt":tss.ChoiceGroup,"sn":"ChoiceGroup"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.InfiniteScrollingListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2325
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"page","dv":-1,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"txt","dv":"","o":true,"pt":$n[0].String,"ps":2}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32,$n[0].Int32,$n[0].String]},{"a":1,"n":"GetSomeItemsAsync","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"page","dv":-1,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"txt","dv":"","o":true,"pt":$n[0].String,"ps":2}],"sn":"GetSomeItemsAsync","rt":$n[2].Task$1(System.Array.type(tss.IC)),"p":[$n[0].Int32,$n[0].Int32,$n[0].String]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ItemsListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2561
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"suffix","dv":"","o":true,"pt":$n[0].String,"ps":1}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32,$n[0].String]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.MasonrySample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 2009
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetCards","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetCards","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ObservableStackSample", function () { return {"nested":[$n[3].ObservableStackSample.StackElement],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 3522
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"AddItem","t":8,"sn":"AddItem","rt":$n[0].Void},{"a":1,"n":"Move","t":8,"pi":[{"n":"oldIdx","pt":$n[0].Int32,"ps":0},{"n":"newIdx","pt":$n[0].Int32,"ps":1}],"sn":"Move","rt":$n[0].Void,"p":[$n[0].Int32,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"Update","t":8,"pi":[{"n":"idx","pt":$n[0].Int32,"ps":0},{"n":"item","pt":$n[3].ObservableStackSample.StackElement,"ps":1}],"sn":"Update","rt":$n[0].Void,"p":[$n[0].Int32,$n[3].ObservableStackSample.StackElement]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true},{"a":1,"n":"_elementIndex","is":true,"t":4,"rt":$n[0].Int32,"sn":"_elementIndex","box":function ($v) { return H5.box($v, System.Int32);}},{"a":1,"n":"_stackElementsList","is":true,"t":4,"rt":tss.ObservableList(tss.ICID),"sn":"_stackElementsList"}]}; }, $n);
    $m("Tesserae.Tests.Samples.ObservableStackSample.StackElement", function () { return {"td":$n[3].ObservableStackSample,"att":1048578,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String],"pi":[{"n":"id","pt":$n[0].String,"ps":0},{"n":"displayName","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":2,"n":"Color","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Color","t":8,"rt":$n[0].String,"fg":"Color"},"fn":"Color"},{"a":2,"n":"ContentHash","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ContentHash","t":8,"rt":$n[0].String,"fg":"ContentHash"},"fn":"ContentHash"},{"a":2,"n":"DisplayName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_DisplayName","t":8,"rt":$n[0].String,"fg":"DisplayName"},"s":{"a":2,"n":"set_DisplayName","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"DisplayName"},"fn":"DisplayName"},{"a":2,"n":"Id","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Id","t":8,"rt":$n[0].String,"fg":"Id"},"s":{"a":2,"n":"set_Id","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Id"},"fn":"Id"},{"a":2,"n":"Identifier","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Identifier","t":8,"rt":$n[0].String,"fg":"Identifier"},"fn":"Identifier"},{"a":1,"backing":true,"n":"<Color>k__BackingField","t":4,"rt":$n[0].String,"sn":"Color"},{"a":1,"backing":true,"n":"<DisplayName>k__BackingField","t":4,"rt":$n[0].String,"sn":"DisplayName"},{"a":1,"backing":true,"n":"<Id>k__BackingField","t":4,"rt":$n[0].String,"sn":"Id"}]}; }, $n);
    $m("Tesserae.Tests.Samples.HashingHelper", function () { return {"att":1048961,"a":2,"s":true,"m":[{"a":2,"n":"Fnv1aHash","is":true,"t":8,"pi":[{"n":"value","pt":$n[0].String,"ps":0}],"sn":"Fnv1aHash","rt":$n[0].Int32,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}}]}; }, $n);
    $m("Tesserae.Tests.Samples.OverflowSetSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 10, Icon: 2707
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableGroupedListSample", function () { return {"nested":[$n[3].SearchableGroupedListSample.SearchableGroupedListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 3605
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetItems","rt":System.Array.type(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem", function () { return {"td":$n[3].SearchableGroupedListSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String],"pi":[{"n":"value","pt":$n[0].String,"ps":0},{"n":"group","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"fn":"Group"},{"a":1,"n":"_component","t":4,"rt":tss.IC,"sn":"_component","ro":true},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableListSample", function () { return {"nested":[$n[3].SearchableListSample.SearchableListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 3605
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetItems","rt":System.Array.type(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableListSample.SearchableListItem", function () { return {"td":$n[3].SearchableListSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"value","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_component","t":4,"rt":tss.IC,"sn":"_component","ro":true},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.StackSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 3522
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.S,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.S,$n[0].Int32]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true},{"a":1,"n":"sampleSortableStackLocalStorageKey","is":true,"t":4,"rt":$n[0].String,"sn":"sampleSortableStackLocalStorageKey"}]}; }, $n);
    $m("Tesserae.Tests.Samples.TimelineSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 4213
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.VirtualizedListSample", function () { return {"nested":[$n[3].VirtualizedListSample.SampleVirtualizedItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2561
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetALotOfItems","t":8,"sn":"GetALotOfItems","rt":$n[4].IEnumerable$1(Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem", function () { return {"td":$n[3].VirtualizedListSample,"att":1048834,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_innerElement","t":4,"rt":HTMLElement,"sn":"_innerElement","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.AccordionSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 26
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ActionButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 5, Icon: 1271
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.AvatarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 4427
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.BadgeSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 4105
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1269
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CarouselSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 3183
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CheckBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 881
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ChoiceGroupSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2561
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ColorPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 2991
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CommandBarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 2707
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CronEditorSample", function () { return {"att":1048833,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 1020
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetObservableExample","t":8,"sn":"GetObservableExample","rt":tss.IC},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DatePickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 689
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DateTimePickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 695
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DeltaComponentSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 3421
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DropdownSample", function () { return {"att":1048833,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 790
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetItemsAsync","t":8,"sn":"GetItemsAsync","rt":$n[2].Task$1(System.Array.type(tss.Dropdown.Item))},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"StartLoadingAsyncDataImmediately","is":true,"t":8,"pi":[{"n":"dropdown","pt":tss.Dropdown,"ps":0}],"sn":"StartLoadingAsyncDataImmediately","rt":tss.Dropdown,"p":[tss.Dropdown]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EditableLabelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 1529
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.GridPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 4084
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetGameOfLifeSample","is":true,"t":8,"sn":"GetGameOfLifeSample","rt":tss.S},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.GridSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 2009
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.HorizontalSeparatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2228
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.LabelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 4164
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.MessageSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1123
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NavbarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2707
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NumberPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 2330
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PaginationSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 2561
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 790
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetComponentPickerItems","t":8,"sn":"GetComponentPickerItems","rt":System.Array.type(Tesserae.Tests.Samples.PickerSampleItemWithComponents)},{"a":1,"n":"GetPickerItems","t":8,"sn":"GetPickerItems","rt":System.Array.type(Tesserae.Tests.Samples.PickerSampleItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSampleItem", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"name","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"IsSelected","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_IsSelected","t":8,"rt":$n[0].Boolean,"fg":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"s":{"a":2,"n":"set_IsSelected","t":8,"p":[$n[0].Boolean],"rt":$n[0].Void,"fs":"IsSelected"},"fn":"IsSelected"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":1,"backing":true,"n":"<IsSelected>k__BackingField","t":4,"rt":$n[0].Boolean,"sn":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSampleItemWithComponents", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[1].UIcons],"pi":[{"n":"name","pt":$n[0].String,"ps":0},{"n":"icon","pt":$n[1].UIcons,"ps":1}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"IsSelected","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_IsSelected","t":8,"rt":$n[0].Boolean,"fg":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"s":{"a":2,"n":"set_IsSelected","t":8,"p":[$n[0].Boolean],"rt":$n[0].Void,"fs":"IsSelected"},"fn":"IsSelected"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":1,"n":"_icon","t":4,"rt":$n[1].UIcons,"sn":"_icon","ro":true,"box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<IsSelected>k__BackingField","t":4,"rt":$n[0].Boolean,"sn":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.SaveButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 1407
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"DynamicTextUpdateSample","t":8,"sn":"DynamicTextUpdateSample","rt":tss.IC},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SavingToastSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 378
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ShowMany","t":8,"sn":"ShowMany","rt":$n[2].Task},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 3605
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SidebarSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 100, Icon: 2707
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"CreateDeepNav","is":true,"t":8,"pi":[{"n":"path","pt":$n[0].String,"ps":0},{"n":"currentDepth","dv":0,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"maxDepth","dv":3,"o":true,"pt":$n[0].Int32,"ps":2}],"sn":"CreateDeepNav","rt":$n[4].IEnumerable$1(Tesserae.ISidebarItem),"p":[$n[0].String,$n[0].Int32,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SidebarSeparatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 99, Icon: 2707
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SkeletonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 3875
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SliderSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 3642
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SplitViewSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 4083
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.StepperSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 30, Icon: 2562
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBlockSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1627
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2332
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBreadcrumbsSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2707
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ToggleSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 3642
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ProgressIndicatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 10, Icon: 336
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ProgressModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 20, Icon: 4677
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SpinnerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 10, Icon: 3875
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SamplesHelper", function () { return {"att":1048961,"a":2,"s":true,"m":[{"a":2,"n":"SampleDo","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleDo","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleDont","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleDont","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleHeader","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].String,"ps":0}],"sn":"SampleHeader","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleSubTitle","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleSubTitle","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleTitle","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleTitle","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"ShowSampleCode","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].String,"ps":0}],"sn":"ShowSampleCode","rt":$n[0].Void,"p":[$n[0].String]}]}; }, $n);
    $m("Tesserae.Tests.Samples.ContextMenuSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 136
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DialogSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 4678
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.FloatSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 0, Icon: 1924
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.LayerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 0, Icon: 4087
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 4679
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PanelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 4678
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PivotSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 4084
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetPivot","t":8,"sn":"GetPivot","rt":tss.Pivot},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PivotSelectorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 4084
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SectionStackSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 498
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.SectionStack,$n[0].Int32]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TutorialModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 2318
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SampleTutorialModal","is":true,"t":8,"sn":"SampleTutorialModal","rt":tss.TutorialModal},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ColorsSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 2991
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetGroupName","t":8,"pi":[{"n":"color","pt":$n[0].ValueTuple$2(System.String,System.String),"ps":0}],"sn":"GetGroupName","rt":$n[0].String,"p":[$n[0].ValueTuple$2(System.String,System.String)]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"RenderColorStack","t":8,"pi":[{"n":"colorName","pt":$n[0].String,"ps":0},{"n":"colorVar","pt":$n[0].String,"ps":1}],"sn":"RenderColorStack","rt":tss.IC,"p":[$n[0].String,$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CommandPaletteSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 31, Icon: 1122
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"BuildActions","is":true,"t":8,"sn":"BuildActions","rt":$n[4].IEnumerable$1(tss.CommandPaletteAction)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DeferSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 3875
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.SectionStack,$n[0].Int32]},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DeferWithProgressSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3875
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EmojiSample", function () { return {"nested":[$n[3].EmojiSample.IconItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3787
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetAllIcons","t":8,"sn":"GetAllIcons","rt":$n[4].IEnumerable$1(Tesserae.Tests.Samples.EmojiSample.IconItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ToValidName","is":true,"t":8,"pi":[{"n":"icon","pt":$n[0].String,"ps":0}],"sn":"ToValidName","rt":$n[0].String,"p":[$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EmojiSample.IconItem", function () { return {"td":$n[3].EmojiSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].Emoji,$n[0].String],"pi":[{"n":"icon","pt":$n[1].Emoji,"ps":0},{"n":"name","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"n":"component","t":4,"rt":tss.IC,"sn":"component","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.FileSelectorAndDropAreaSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 20, Icon: 1853
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NodeViewSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 2871
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ThemeColorsSample", function () { return {"nested":[$n[3].ThemeColorsSample.ColorListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 2991
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"DumpTheme","is":true,"t":8,"sn":"DumpTheme","rt":$n[0].Void},{"a":2,"n":"LightDiff","is":true,"t":8,"pi":[{"n":"from","pt":$n[0].String,"ps":0},{"n":"to","pt":$n[0].String,"ps":1}],"sn":"LightDiff","rt":$n[0].Double,"p":[$n[0].String,$n[0].String],"box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content"}]}; }, $n);
    $m("Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem", function () { return {"td":$n[3].ThemeColorsSample,"att":1048578,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"themeName","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":1,"n":"ColorSquare","is":true,"t":8,"pi":[{"n":"color","pt":$n[0].String,"ps":0}],"sn":"ColorSquare","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[3].ThemeColorsSample.ColorListItem,"ps":0},{"n":"columnSortingKey","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[3].ThemeColorsSample.ColorListItem,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"listItemIndex","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[4].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"createGridCellExpression","pt":Function,"ps":1}],"sn":"Render","rt":$n[4].IEnumerable$1(tss.IC),"p":[$n[4].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"ThemeName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ThemeName","t":8,"rt":$n[0].String,"fg":"ThemeName"},"fn":"ThemeName"},{"a":2,"n":"Mapping","is":true,"t":4,"rt":$n[4].Dictionary$2(System.String,System.Collections.Generic.Dictionary$2(System.String,System.String)),"sn":"Mapping"},{"a":1,"backing":true,"n":"<ThemeName>k__BackingField","t":4,"rt":$n[0].String,"sn":"ThemeName"}]}; }, $n);
    $m("Tesserae.Tests.Samples.TippySample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 1145
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ToastSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 20, Icon: 593
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.UIconsSample", function () { return {"nested":[$n[3].UIconsSample.IconItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3183
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetAllIcons","t":8,"sn":"GetAllIcons","rt":$n[4].IEnumerable$1(Tesserae.Tests.Samples.UIconsSample.IconItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ToValidName","is":true,"t":8,"pi":[{"n":"icon","pt":$n[0].String,"ps":0}],"sn":"ToValidName","rt":$n[0].String,"p":[$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.UIconsSample.IconItem", function () { return {"td":$n[3].UIconsSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].UIcons,$n[0].String],"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0},{"n":"name","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"n":"component","t":4,"rt":tss.IC,"sn":"component","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ValidatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 876
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
});
