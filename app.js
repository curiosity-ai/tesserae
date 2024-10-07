/**
 * @compiler H5 24.8.51427+a2a4cc25c0576f2dba2db3f5b79247ea92ffd095
 */
H5.assemblyVersion("Tesserae.Tests","2024.10.52697.0");
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

            sidebar.AddHeader(new Tesserae.SidebarText("header", "tesserae", "TSS", "tss-fontsize-large", "tss-fontweight-bold").PT(16).PB(16).PL(12));

            var pageContent = tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.HS(tss.Sidebar, sidebar), tss.ICX.Grow(tss.IDefer, tss.ICX.W(tss.IDefer, tss.ICX.HS(tss.IDefer, tss.UI.DeferSync$3(Tesserae.Tests.Sample, currentPage, function (page) {
                return page == null ? H5.cast(Tesserae.Tests.App.CenteredCardWithBackground(tss.UI.TextBlock("Select an item")), tss.IC) : tss.ICTX.Children$6(tss.S, tss.ScrollBar.ScrollY(tss.S, tss.ICX.S(tss.S, tss.UI.VStack())), [tss.ICX.WS(tss.IC, page.ContentGenerator())]);
            })), 1))]));

            document.body.appendChild(pageContent.Render());


            var samples = System.Linq.Enumerable.from(H5.Reflection.getAssemblyTypes(H5.Reflection.getTypeAssembly(Tesserae.Tests.ISample)), System.Type).where(function (t) {
                    return H5.Reflection.isAssignableFrom(Tesserae.Tests.ISample, t) && !H5.Reflection.isInterface(t);
                }).select(function (sampleType) {
                var sg = H5.as(System.Linq.Enumerable.from(H5.Reflection.getAttributes(sampleType, Tesserae.Tests.SampleDetailsAttribute, true), System.Object).firstOrDefault(null, null), Tesserae.Tests.SampleDetailsAttribute);
                var group = H5.is(sg, System.Object) ? sg.Group : "Others";
                var order = H5.is(sg, System.Object) ? sg.Order : 0;
                var icon = H5.is(sg, System.Object) ? sg.Icon : 779;
                return new Tesserae.Tests.Sample(H5.Reflection.getTypeName(sampleType), System.String.replaceAll(H5.Reflection.getTypeName(sampleType), "Sample", ""), group, order, icon, function () {
                    return H5.as(H5.createInstance(sampleType), tss.IC);
                });
            }).toDictionary(function (s) {
                    return s.Name;
                }, function (s) {
                    return s;
                }, System.String, Tesserae.Tests.Sample);

            sidebar.AddHeader(new Tesserae.SidebarButton.$ctor2("SOURCE_CODE", 696, "Source Code", [new Tesserae.SidebarCommand.$ctor3(184).Tooltip$1("Open repository on GitHub").OnClick(function () {
                window.open("https://github.com/curiosity-ai/tesserae", "_blank");
            })]).CommandsAlwaysVisible().OnOpenIconClick(function () {
                tss.UI.Toast().Success("You clicked on the icon");
            }));

            var openClose = new Tesserae.SidebarCommand.$ctor3(83).Tooltip$1("Close Sidebar");
            var v = { };

            var sidebarOpenState = System.Boolean.tryParse(localStorage.getItem(Tesserae.Tests.App._sidebarOpenStateKey), v) ? v.v : true;

            sidebar.Closed(!sidebarOpenState);

            openClose.OnClick(function () {
                sidebar.Toggle();

                if (sidebar.IsClosed) {
                    openClose.SetIcon$1(84).Tooltip$1("Open Sidebar");
                    localStorage.setItem(Tesserae.Tests.App._sidebarOpenStateKey, System.Boolean.toString((false)));
                } else {
                    openClose.SetIcon$1(83).Tooltip$1("Close Sidebar");
                    localStorage.setItem(Tesserae.Tests.App._sidebarOpenStateKey, System.Boolean.toString((true)));
                }
            });

            var lightDark = new Tesserae.SidebarCommand.$ctor3(3267).Tooltip$1("Light Mode");

            lightDark.OnClick(function () {
                if (tss.UI.Theme.IsDark) {
                    tss.UI.Theme.Light();
                    lightDark.SetIcon$1(3267).Tooltip$1("Light Mode");
                } else {
                    tss.UI.Theme.Dark();
                    lightDark.SetIcon$1(2298).Tooltip$1("Dark Mode");
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

            var dotsMenu = new Tesserae.SidebarCommand.$ctor3(2219).OnClickMenu(function () {
                return System.Array.init([new Tesserae.SidebarButton.$ctor6("MANAGE_ACCOUNT", 3608, "Manage Account"), new Tesserae.SidebarButton.$ctor6("PREFERENCES", 2935, "Preferences"), new Tesserae.SidebarButton.$ctor6("DELETE", 3506, "Delete Account"), new Tesserae.SidebarCommands("EMOTIONS", [new Tesserae.SidebarCommand.$ctor1(9), new Tesserae.SidebarCommand.$ctor1(51), new Tesserae.SidebarCommand.$ctor1(53)]), new Tesserae.SidebarCommands("ADD_DELETE", [new Tesserae.SidebarCommand.$ctor3(2628).Primary(), new Tesserae.SidebarCommand.$ctor3(3506).Danger()]).AlignEnd(), new Tesserae.SidebarButton.$ctor6("SIGNOUT", 2996, "Sign Out")], Tesserae.ISidebarItem);
            });

            var commandsEndAligned = new Tesserae.SidebarCommands("SETTINGS", [fireworks, dotsMenu]).AlignEnd();

            var commandSidebarconfig = new Tesserae.SidebarCommands("OPENCLOSE", [openClose]);


            sidebar.AddFooter(new Tesserae.SidebarNav.$ctor1("EMPTY_NAV", 853, "Empty Nav", true).ShowDotIfEmpty().OnOpenIconClick$1(function (e, m) {
                tss.UI.Toast().Success("You clicked on the icon!");
            }));

            sidebar.AddFooter(commands);
            sidebar.AddFooter(commandsEndAligned);
            sidebar.AddFooter(commandSidebarconfig);

            sidebar.AddFooter(new Tesserae.SidebarButton.$ctor3("CURIOSITY_REF", new tss.ImageIcon("/assets/img/curiosity-logo.svg"), "By Curiosity", new Tesserae.SidebarBadge.ctor("+3").Foreground(tss.UI.Theme.Primary.Foreground).Background(tss.UI.Theme.Primary.Background), [new Tesserae.SidebarCommand.$ctor3(184).OnClick(function () {
                window.open("https://github.com/curiosity-ai/tesserae", "_blank");
            })]).Tooltip$1("Made with \u2764 by Curiosity").OnClick(function () {
                window.open("https://curiosity.ai", "_blank");
            }));


            var groupIndex = 0;

            $t = H5.getEnumerator(System.Linq.Enumerable.from(samples.Values, Tesserae.Tests.Sample).groupBy(function (s) {
                    return s.Group;
                }));
            try {
                while ($t.moveNext()) {
                    var group = $t.Current;
                    var groupKey = (group.key() || "") + H5.identity(groupIndex, ((groupIndex = (groupIndex + 1) | 0)));

                    var nav = new Tesserae.SidebarNav.$ctor2(group.key(), 442, group.key(), false).OnClick$1(function (n) {
                        n.Toggle();
                    });
                    allSidebarItems.add(nav);
                    sidebar.AddContent(nav);

                    var itemIndex = 0;

                    $t1 = H5.getEnumerator(group.orderBy(function (s) {
                        return s.Order;
                    }).thenBy(function (s) {
                        return s.Name.toLowerCase();
                    }));
                    try {
                        while ($t1.moveNext()) {
                            var item = { v : $t1.Current };
                            var sidebarItem = new Tesserae.SidebarButton.$ctor6((item.v.Name || "") + H5.identity(itemIndex, ((itemIndex = (itemIndex + 1) | 0))), item.v.Icon, item.v.Name, [new Tesserae.SidebarCommand.$ctor3(3194).Tooltip$1("Show sample code").OnClick((function ($me, item) {
                                return function () {
                                    Tesserae.Tests.Samples.SamplesHelper.ShowSampleCode(item.v.Name);
                                };
                            })(this, item)), new Tesserae.SidebarCommand.$ctor3(184).Tooltip$1("Open in new tab").OnClick((function ($me, item) {
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


                            nav.Add(sidebarItem);
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
                if (itemOrderObj == null) {
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
                    tss.Router.Register(System.String.format("#/view/{0}", [kv.v.key]), (function ($me, kv) {
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
            ctor: function (fileIcon, fileName, dateModified, modifiedBy, fileSize) {
                this.$initialize();
                this.FileIcon = fileIcon;
                this.FileName = fileName;
                this.DateModified = dateModified;
                this.ModifiedBy = modifiedBy;
                this.FileSize = fileSize;
            }
        },
        methods: {
            OnListItemClick: function (listItemIndex) {
                alert(System.String.format("You clicked me! List item index: {0}, my name is {1}", H5.box(listItemIndex, System.Int32), this.FileName));
            },
            CompareTo: function (other, columnSortingKey) {
                if (other == null) {
                    throw new System.ArgumentException.$ctor1("other");
                }

                if (System.String.equals(columnSortingKey, "FileIcon")) {
                    return H5.compare(this.FileIcon, H5.box(other.FileIcon, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons)));
                }

                if (System.String.equals(columnSortingKey, "FileName")) {
                    return System.String.compare(this.FileName, other.FileName, 4);
                }

                if (System.String.equals(columnSortingKey, "DateModified")) {
                    return H5.compare(this.DateModified, other.DateModified);
                }

                if (System.String.equals(columnSortingKey, "ModifiedBy")) {
                    return System.String.compare(this.FileName, other.FileName, 4);
                }

                if (System.String.equals(columnSortingKey, "FileSize")) {
                    return H5.compare(this.FileSize, other.FileSize);
                }

                throw new System.InvalidOperationException.$ctor1(System.String.format("Can not match {0} to current list item", [columnSortingKey]));
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
                                                return tss.UI.Icon$2(this.FileIcon);
                                            }));
                                            $s = 1;
                                            return true;
                                    }
                                    case 1: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 1, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(this.FileName);
                                            }));
                                            $s = 2;
                                            return true;
                                    }
                                    case 2: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 2, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(System.DateTime.format(this.DateModified, "d"));
                                            }));
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 3, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(this.ModifiedBy);
                                            }));
                                            $s = 4;
                                            return true;
                                    }
                                    case 4: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 4, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(System.Double.format(this.FileSize));
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
            ChoiceGroup: null,
            Dropdown: null,
            Toggle: null
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
            OnListItemClick: function (listItemIndex) { },
            CompareTo: function (other, columnSortingKey) {
                return 0;
            },
            WithIcon: function (icon) {
                this.Icon = icon;
                return this;
            },
            WithCheckBox: function (checkBox) {
                this.CheckBox = checkBox;
                return this;
            },
            WithName: function (name) {
                this.Name = name;
                return this;
            },
            WithButton: function (button) {
                this.Button = button;
                return this;
            },
            WithChoiceGroup: function (choiceGroup) {
                this.ChoiceGroup = choiceGroup;

                return this;
            },
            WithDropdown: function (dropdown) {
                this.Dropdown = dropdown;
                return this;
            },
            WithToggle: function (toggle) {
                this.Toggle = toggle;
                return this;
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
                                                return tss.UI.Icon$2(this.Icon);
                                            }));
                                            $s = 1;
                                            return true;
                                    }
                                    case 1: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 1, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.CheckBox;
                                            }));
                                            $s = 2;
                                            return true;
                                    }
                                    case 2: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 2, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return tss.UI.TextBlock(this.Name);
                                            }));
                                            $s = 3;
                                            return true;
                                    }
                                    case 3: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 3, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.Button;
                                            }));
                                            $s = 4;
                                            return true;
                                    }
                                    case 4: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 4, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.ChoiceGroup;
                                            }));
                                            $s = 5;
                                            return true;
                                    }
                                    case 5: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 5, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.Dropdown;
                                            }));
                                            $s = 6;
                                            return true;
                                    }
                                    case 6: {
                                        $en.current = createGridCellExpression(System.Array.getItem(columns, 6, tss.IDetailsListColumn), H5.fn.bind(this, function () {
                                                return this.Toggle;
                                            }));
                                            $s = 7;
                                            return true;
                                    }
                                    case 7: {

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
                return tss.ICTX.Children$6(tss.S, tss.UI.HStack().AlignContent("center"), [tss.ICX.MinWidth(tss.Icon, tss.UI.Icon$2(this._icon), tss.usX.px$1(16)), tss.UI.TextBlock(this.Name)]);
            }
        }
    });

    H5.define("Tesserae.Tests.Samples.SamplesHelper", {
        statics: {
            methods: {
                SampleHeader: function (sampleType) {
                    var text = System.String.replaceAll(sampleType, "Sample", "");

                    return tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack().Horizontal()), [tss.ITFX.Bold(tss.txt, tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock(text))), tss.ICX.Grow(tss.Raw, tss.UI.Raw$1(), 1), tss.UI.Button$1().SetIcon$1(3194).SetTitle("View code for this sample").OnClick$1(function () {
                        Tesserae.Tests.Samples.SamplesHelper.ShowSampleCode(sampleType);
                    })]);
                },
                ShowSampleCode: function (sampleType) {
                    var text = System.String.replaceAll(sampleType, "Sample", "");
                    tss.LayerExtensions.Content(tss.Modal, tss.ICX.W$1(tss.Modal, tss.UI.Modal((text || "") + " sample code").LightDismiss(), tss.usX.vh$1(80)).ShowCloseButton(), tss.ICX.H$1(tss.TextArea, tss.ICX.WS(tss.TextArea, tss.UI.TextArea$1(Tesserae.Tests.SamplesSourceCode.GetCodeForSample(sampleType))), tss.usX.vh$1(80))).Show();
                },
                SampleTitle: function (text) {
                    return tss.ICX.PaddingBottom(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(text))), tss.usX.px$1(16));
                },
                SampleSubTitle: function (text) {
                    return tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock(text))), tss.usX.px$1(16));
                },
                SampleDo: function (text) {
                    return tss.UI.Label$2(tss.ICX.PaddingRight(tss.Raw, tss.UI.Raw$2(tss.UI.I(tss.UI._$2("las la-check", void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                        s.color = "#107c10";
                    }))), tss.usX.px$1(8))).SetContent(tss.UI.TextBlock(text)).Inline();
                },
                SampleDont: function (text) {
                    return tss.UI.Label$2(tss.ICX.PaddingRight(tss.Raw, tss.UI.Raw$2(tss.UI.I(tss.UI._$2("las la-times", void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
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
                this._component = tss.UI.Card(tss.txtX.NonSelectable(tss.txt, tss.UI.TextBlock(value)));

                this.Group = group;
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
                this._component = tss.UI.Card(tss.txtX.NonSelectable(tss.txt, tss.UI.TextBlock(value)));
            }
        },
        methods: {
            IsMatch: function (searchTerm) {
                return System.String.contains(this._value,searchTerm);
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
                        return tss.UI.Raw$2(tss.UI.Div$2(tss.UI._$2(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                            s.width = "50px";
                            s.height = "49px";
                        })));
                    }

                    return tss.UI.Raw$2(tss.UI.Div$2(tss.UI._$2(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
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
                this._innerElement = tss.UI.Div$2(tss.UI._$2(void 0, void 0, void 0, void 0, void 0, void 0, text, void 0, void 0, void 0, void 0, void 0, void 0, void 0, function (s) {
                    s.display = "block";
                    s.textAlign = "center";
                    s.height = "63px";
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
                var btn11 = { };
                var btn2 = { };
                var btn3 = { };
                var btn4 = { };
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ActionButtonSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()).Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do")]), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Default ActionButton")), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Var(tss.ActionButton, tss.UI.ActionButton("Action1"), btn1).OnClickDisplay(function (s, e) {
                    alert("Clicked1 display!");
                }).OnClickAction(function (s, e) {
                    alert("Clicked1 action!");
                }), tss.UI.Var(tss.ActionButton, tss.UI.ActionButton("Action1.1", 566, "fi-rr-", void 0, "tss-fontsize-small"), btn11).Primary().OnClickDisplay(function (s, e) {
                    alert("Clicked1 display!");
                }).OnClickAction(function (s, e) {
                    alert("Clicked1 action!");
                }), tss.UI.Var(tss.ActionButton, tss.UI.ActionButton$1("Action  2", 566, "fi-rr-", void 0, "tss-fontsize-small", "fi-rr-", 72, void 0, "tss-fontsize-small"), btn2).Danger().OnClickDisplay(function (s, e) {
                    alert("Clicked2 display!");
                }).OnClickAction(function (s, e) {
                    alert("Clicked2 action!");
                }).ModifyActionButton(function (btn) {
                    tss.ICX.Tooltip$1(tss.Raw, tss.UI.Raw$3(btn), "Action!", "none", "top", 250, 0, false, 350, true, void 0, void 0);
                }), tss.UI.Var(tss.ActionButton, tss.UI.ActionButton("Action 3 but it is a really long button text"), btn3).Primary().OnClickDisplay(function (s, e) {
                    alert("Clicked3 display!");
                }).OnClickAction(function (s, e) {

                    var hideAction = { v : null };

                    var tt = tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.UI.Button$1("1").OnClick$1(function () {
                        tss.UI.Toast().Information("1");
                        !H5.staticEquals(hideAction.v, null) ? hideAction.v() : null;
                    }), tss.UI.Button$1("2").OnClick$1(function () {
                        tss.UI.Toast().Information("2");
                        !H5.staticEquals(hideAction.v, null) ? hideAction.v() : null;
                    }), tss.UI.Button$1("3").OnClick$1(function () {
                        tss.UI.Toast().Information("3");
                        !H5.staticEquals(hideAction.v, null) ? hideAction.v() : null;
                    })]).Render();
                    tss.tippy.ShowFor(s, tt, hideAction, "none", "bottom-end", 0, 0, 350, true, null);

                }), tss.UI.Var(tss.ActionButton, tss.UI.ActionButton$2(tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Icon$2(190), tss.UI.TextBlock("Lorem ipsum")]), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("Subtitle")), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("Subtitle2")), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("Subtitle4"))])), btn4).OnClickDisplay(function (s, e) {
                    alert("Clicked4 display!");
                }).OnClickAction(function (s, e) {
                    alert("Clicked4 action!");
                })])]));
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
                var msg = { };
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("BreadcrumbSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Breadcrumbs should be used as a navigational aid in your app or site. They indicate the current page\u2019s location within a hierarchy and help the user understand where they are in relation to the rest of that hierarchy. They also afford one-click access to higher levels of that hierarchy."), tss.UI.TextBlock("Breadcrumbs are typically placed, in horizontal form, under the masthead or navigation of an experience, above the primary content area.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Place Breadcrumbs at the top of a page, above a list of items, or above the main content of a page.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use Breadcrumbs as a primary way to navigate an app or site.")])])])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(450)), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Label$1("Selected: ").SetContent(tss.UI.Var(tss.txt, tss.UI.TextBlock$1(), msg)), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("All Visible")), tss.ICX.PaddingBottom(tss.Breadcrumb, tss.ICX.PaddingTop(tss.Breadcrumb, tss.UI.Breadcrumb(), tss.usX.px$1(16)).SetOverflowIndex(-1), tss.usX.px$1(16)).Items([tss.UI.Crumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Crumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }).Disabled(), tss.UI.Crumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Crumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Crumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Crumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("All Visible, Small")), tss.ICX.PaddingBottom(tss.Breadcrumb, tss.ICX.PaddingTop(tss.Breadcrumb, tss.UI.Breadcrumb().Small(), tss.usX.px$1(16)), tss.usX.px$1(16)).Items([tss.UI.Crumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Crumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }).Disabled(), tss.UI.Crumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Crumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Crumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Crumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 200px")), tss.ICX.MaxWidth(tss.Breadcrumb, tss.ICX.PaddingBottom(tss.Breadcrumb, tss.ICX.PaddingTop(tss.Breadcrumb, tss.UI.Breadcrumb(), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(200)).Items([tss.UI.Crumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Crumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Crumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Crumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Crumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Crumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 200px, Small")), tss.ICX.MaxWidth(tss.Breadcrumb, tss.ICX.PaddingBottom(tss.Breadcrumb, tss.ICX.PaddingTop(tss.Breadcrumb, tss.UI.Breadcrumb(), tss.usX.px$1(16)), tss.usX.px$1(16)).Small(), tss.usX.px$1(200)).Items([tss.UI.Crumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Crumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Crumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Crumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Crumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Crumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 300px")), tss.ICX.MaxWidth(tss.Breadcrumb, tss.ICX.PaddingBottom(tss.Breadcrumb, tss.ICX.PaddingTop(tss.Breadcrumb, tss.UI.Breadcrumb(), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(300)).Items([tss.UI.Crumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Crumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Crumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Crumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Crumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Crumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 300px, from second, custom chevron")), tss.ICX.MaxWidth(tss.Breadcrumb, tss.ICX.PaddingBottom(tss.Breadcrumb, tss.ICX.PaddingTop(tss.Breadcrumb, tss.UI.Breadcrumb(), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(300)).SetChevron(2596).SetOverflowIndex(1).Items([tss.UI.Crumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Crumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Crumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Crumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Crumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Crumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })])]));
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
                var btn1 = { };
                var btn2 = { };
                var btn3 = { };
                var iconBtn1 = { };
                var iconBtn2 = { };
                var iconBtn3 = { };
                var iconBtn4 = { };
                var spinBtn1 = { };
                var spinBtn2 = { };
                var spinBtn3 = { };
                var spinBtn4 = { };
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ButtonSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog."), tss.UI.TextBlock("When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements."), tss.UI.TextBlock("While buttons can technically be used to navigate a user to another part of the experience, this is not recommended unless that navigation is part of an action or their flow.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()).Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Make sure the label conveys a clear purpose of the button to the user."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Button labels must describe the action the button will perform and should include a verb. Use concise, specific, self-explanatory labels, usually a single word."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Buttons should always include a noun if there is any room for interpretation about what the verb operates on."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Consider the affect localization will have on the button and what will happen to components around it."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("If the button\u2019s label content is dynamic, consider how the button will resize and what will happen to components around it."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use only a single line of text in the label of the button."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Expose only one or two buttons to the user at a time, for example, \"Accept\" and \"Cancel\". If you need to expose more actions to the user, consider using checkboxes or radio buttons from which the user can select actions, with a single command button to trigger those actions."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Show only one primary button that inherits theme color at rest state. In the event there are more than two buttons with equal priority, all buttons should have neutral backgrounds."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("\"Submit\", \"OK\", and \"Apply\" buttons should always be styled as primary buttons. When \"Reset\" or \"Cancel\" buttons appear alongside one of the above, they should be styled as secondary buttons."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Default buttons should always perform safe operations. For example, a default button should never delete."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use task buttons to cause actions that complete a task or cause a transitional task. Do not use buttons to toggle other UX in the same context. For example, a button may be used to open an interface area but should not be used to open an additional set of components in the same interface.")]), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use generic labels like \"Ok, \" especially in the case of an error; errors are never \"Ok.\""), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t place the default focus on a button that destroys data. Instead, place the default focus on the button that performs the \"safe act\" and retains the content (i.e. \"Save\") or cancels the action (i.e. \"Cancel\")."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t use a button to navigate to another place, use a link instead. The exception is in a wizard where \"Back\" and \"Next\" buttons may be used."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t put too much text in a button - try to keep the length of your text to a minimum."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't put anything other than text in a button.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Default Button")), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Tooltip$1(tss.Button, tss.UI.Var(tss.Button, tss.UI.Button$1(), btn1).SetText("Standard"), "This is a standard button").OnClick(function (s, e) {
                    alert("Clicked!");
                }), tss.ICX.Tooltip$1(tss.Button, tss.UI.Var(tss.Button, tss.UI.Button$1(), btn2).SetText("Primary"), "This is a primary button").Primary().OnClick(function (s, e) {
                    alert("Clicked!");
                }), tss.ICX.Tooltip(tss.Button, tss.UI.Var(tss.Button, tss.UI.Button$1(), btn3).SetText("Link"), tss.UI.Button$1("This is a link button with a button tooltip").Primary().OnClick(function (_, __) {
                    tss.UI.Toast().Success("You clicked here");
                }), true, "none", "top", 250, 0, true, false, 350, true, false, void 0, void 0).Link().OnClick(function (s, e) {
                    alert("Clicked!");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Icon Button")), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Var(tss.Button, tss.UI.Button$1(), iconBtn1).SetText("Confirm").SetIcon$1(740).Success().OnClick(function (s, e) {
                    alert("Clicked!");
                }), tss.UI.Var(tss.Button, tss.UI.Button$1(), iconBtn2).SetText("Delete").SetIcon$1(3506).Danger().OnClick(function (s, e) {
                    alert("Clicked!");
                }), tss.UI.Var(tss.Button, tss.UI.Button$1(), iconBtn3).SetText("Primary").SetIcon$1(2258).Primary().OnClick(function (s, e) {
                    alert("Clicked!");
                }), tss.UI.Var(tss.Button, tss.UI.Button$1(), iconBtn4).SetText("Copy date").SetIcon$1(566).OnClick(function (s, e) {
                    tss.Clipboard.Copy(System.DateTime.format(System.DateTime.getNow()));
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner Button")), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Var(tss.Button, tss.UI.Button$1(), spinBtn1).SetText("Spin").OnClickSpinWhile(function () {
                    var $s = 0,
                        $t1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            $t1 = System.Threading.Tasks.Task.delay(1000);
                                            $s = 1;
                                            if ($t1.isC()) {
                                                continue;
                                            }
                                            $t1.c($ab);
                                            return;
                                        }
                                        case 1: {
                                            $t1.gAR();
                                            $tcs.sR(null);
                                            return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                }), tss.UI.Var(tss.Button, tss.UI.Button$1(), spinBtn2).SetText("Spin with text").OnClickSpinWhile(function () {
                    var $s = 0,
                        $t1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            $t1 = System.Threading.Tasks.Task.delay(1000);
                                            $s = 1;
                                            if ($t1.isC()) {
                                                continue;
                                            }
                                            $t1.c($ab);
                                            return;
                                        }
                                        case 1: {
                                            $t1.gAR();
                                            $tcs.sR(null);
                                            return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                }, "loading..."), tss.UI.Var(tss.Button, tss.UI.Button$1(), spinBtn3).SetText("Spin & Error").OnClickSpinWhile(function () {
                    var $s = 0,
                        $t1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            $t1 = System.Threading.Tasks.Task.delay(1000);
                                            $s = 1;
                                            if ($t1.isC()) {
                                                continue;
                                            }
                                            $t1.c($ab);
                                            return;
                                        }
                                        case 1: {
                                            $t1.gAR();
                                            throw new System.Exception("Error!");
                                            $tcs.sR(null);
                                            return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                }, void 0, function (b, e) {
                    spinBtn3.v.SetText("Failed: " + (e.Message || "")).SetIcon$1(3536).DangerLink();
                }), tss.UI.Var(tss.Button, tss.UI.Button$1(), spinBtn4).SetText("Spin with text & Error").OnClickSpinWhile(function () {
                    var $s = 0,
                        $t1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            $t1 = System.Threading.Tasks.Task.delay(1000);
                                            $s = 1;
                                            if ($t1.isC()) {
                                                continue;
                                            }
                                            $t1.c($ab);
                                            return;
                                        }
                                        case 1: {
                                            $t1.gAR();
                                            throw new System.Exception("Error!");
                                            $tcs.sR(null);
                                            return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                }, "loading...", function (b, e) {
                    spinBtn4.v.SetText("Failed: " + (e.Message || "")).SetIcon$1(3536).DangerLink();
                })]), tss.UI.Toggle$1("Disable buttons").Checked().OnChange(function (s, e) {
                    var $t, $t1, $t2, $t3, $t4, $t5;
                    btn1.v.IsEnabled = ($t = ($t1 = ($t2 = ($t3 = ($t4 = ($t5 = s.IsChecked, iconBtn4.v.IsEnabled = $t5, $t5), iconBtn3.v.IsEnabled = $t4, $t4), iconBtn2.v.IsEnabled = $t3, $t3), iconBtn1.v.IsEnabled = $t2, $t2), btn3.v.IsEnabled = $t1, $t1), btn2.v.IsEnabled = $t, $t);
                })]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("CheckBoxSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A CheckBox is a UI element that allows users to switch between two mutually exclusive options (checked or unchecked, on or off) through a single click or tap. It can also be used to indicate a subordinate setting or preference when paired with another control."), tss.UI.TextBlock("A CheckBox is used to select or deselect action items. It can be used for a single item or for a list of multiple items that a user can choose from. The control has two selection states: unselected and selected."), tss.UI.TextBlock("Use a single CheckBox for a subordinate setting, such as with a \"Remember me ? \" login scenario or with a terms of service agreement."), tss.UI.TextBlock("For a binary choice, the main difference between a CheckBox and a toggle switch is that the CheckBox is for status and the toggle switch is for action. You can delay committing a CheckBox interaction (as part of a form submit, for example), while you should immediately commit a toggle switch interaction. Also, only CheckBoxes allow for multi-selection."), tss.UI.TextBlock("Use multiple CheckBoxes for multi-select scenarios in which a user chooses one or more items from a group of choices that are not mutually exclusive.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Allow users to choose any combination of options when several CheckBoxes are grouped together.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use a CheckBox as an on/off control. Instead use a toggle switch."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t use a CheckBox when the user can choose only one option from the group, use radio buttons instead."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't put two groups of CheckBoxes next to each other. Separate the two groups with labels.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic CheckBoxes")), tss.UI.CheckBox$1("Unchecked checkbox"), tss.UI.CheckBox$1("Checked checkbox").Checked(), tss.UI.CheckBox$1("Disabled checkbox").Disabled(), tss.UI.CheckBox$1("Disabled checked checkbox").Checked().Disabled()]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ChoiceGroupSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The ChoiceGroup component, also known as radio buttons, let users select one option from two or more choices. Each option is represented by one ChoiceGroup button; a user can select only one ChoiceGroup in a button group."), tss.UI.TextBlock("ChoiceGroup emphasize all options equally, and that may draw more attention to the options than necessary. Consider using other controls, unless the options deserve extra attention from the user. For example, if the default option is recommended for most users in most situations, use a Dropdown component instead."), tss.UI.TextBlock("If there are only two mutually exclusive options, combine them into a single Checkbox or Toggle switch. For example, use a Checkbox for \"I agree\" instead of ChoiceGroup buttons for \"I agree\" and \"I don't agree.\"")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use when there are 2-7 options, if you have enough screen space and the options are important enough to be a good use of that screen space. Otherwise, use a Checkbox or Dropdown list."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use on wizard pages to make the alternatives clear, even if a Checkbox is otherwise acceptable."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("List the options in a logical order, such as most likely to be selected to least, simplest operation to most complex, or least risk to most. Alphabetical ordering is not recommended because it is language dependent and therefore not localizable."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("If none of the options is a valid choice, add another option to reflect this choice, such as \"None\" or \"Does not apply\"."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Select the safest (to prevent loss of data or system access) and most secure and private option as the default. If safety and security aren't factors, select the most likely or convenient option."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Align radio buttons vertically instead of horizontally, if possible. Horizontal alignment is harder to read and localize.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Use when the options are numbers that have fixed steps, like 10, 20, 30. Use a Slider component instead."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Use if there are more than 7 options, use a Dropdown instead."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Nest with other ChoiceGroup or CheckBoxes. If possible, keep all the options at the same level.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Default ChoiceGroup")), tss.UI.ChoiceGroup().Choices([tss.UI.Choice("Option A"), tss.UI.Choice("Option B"), tss.UI.Choice("Option C").Disabled(), tss.UI.Choice("Option D")]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Required ChoiceGroup with a custom label")), tss.UI.ChoiceGroup("Custom label").Required().Choices([tss.UI.Choice("Option A"), tss.UI.Choice("Option B"), tss.UI.Choice("Option C").Disabled(), tss.UI.Choice("Option D")]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Horizontal ChoiceGroup")), tss.UI.ChoiceGroup().Horizontal().Choices([tss.UI.Choice("Option A"), tss.UI.Choice("Option B"), tss.UI.Choice("Option C").Disabled(), tss.UI.Choice("Option D")])]));
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
                var colorPicker1 = { };
                var button1 = { };
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ColorPickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The ColorPicker allows users to pick a color from a native browser widget. Unless specified, black is the default color upon render of the component"), tss.UI.Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/color", "Please see here for further information.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic ColorPicker")), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Standard").SetContent(tss.ICTX.Children$6(tss.S, tss.ICX.Stretch(tss.S, tss.UI.HStack()), [tss.UI.Var(tss.ColorPicker, tss.ICX.Width(tss.ColorPicker, tss.UI.ColorPicker(), tss.usX.percent$1(10)), colorPicker1), tss.UI.Var(tss.Button, tss.UI.Button$1().SetText("Click me!"), button1)])), tss.ICX.Width(tss.Label, tss.UI.Label$1("With preset color").SetContent(tss.UI.ColorPicker(tss.Color.FromString("#0078d4"))), tss.usX.percent$1(10)), tss.ICX.Width(tss.Label, tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.ColorPicker().Disabled()), tss.usX.percent$1(10)), tss.ICX.Width(tss.Label, tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.ColorPicker()), tss.usX.percent$1(10)), tss.ICX.Width(tss.ColorPicker, tss.UI.ColorPicker().Required(), tss.usX.percent$1(10)), tss.ICX.Width(tss.Label, tss.UI.Label$1("With error message").SetContent(tss.ICVX.IsInvalid(tss.ColorPicker, tss.ICVX.Error(tss.ColorPicker, tss.UI.ColorPicker(), "Error message"))), tss.usX.percent$1(10)), tss.ICX.Width(tss.Label, tss.UI.Label$1("With validation for light color").SetContent(tss.vX.Validation(tss.ColorPicker, tss.UI.ColorPicker(), tss.Validation.LightColor)), tss.usX.percent$1(10)), tss.ICX.Width(tss.Label, tss.UI.Label$1("With validation for dark color").SetContent(tss.vX.Validation(tss.ColorPicker, tss.UI.ColorPicker(), tss.Validation.DarkColor)), tss.usX.percent$1(10))])]));

                colorPicker1.v.OnChange(function (_, __) {
                    button1.v.Background = colorPicker1.v.Text;
                });
                button1.v.OnClick(function (_, __) {
                    window.alert(System.String.format("{0}, {1}", colorPicker1.v.Text, colorPicker1.v.Color.ToHex()));
                });
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
                var d = tss.UI.ContextMenu();
                var msg = tss.UI.TextBlock$1();
                var msg2 = tss.UI.TextBlock$1();

                var cmsub = tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem("Edit Sub").OnClick$2(function (s2, e2) {
                    tss.UI.Toast().Information("Clicked: Edit Sub");
                }), tss.UI.ContextMenuItem("Properties Sub").OnClick$2(function (s2, e2) {
                    tss.UI.Toast().Information("Clicked: Properties Sub");
                }), tss.UI.ContextMenuItem("Header Sub").Header(), tss.UI.ContextMenuItem("Disabled Sub").Disabled(), tss.UI.ContextMenuItem("Link Sub").OnClick$2(function (s2, e2) {
                    tss.UI.Toast().Information("Clicked: Link Sub");
                })]);

                var cmsub2 = tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem("Edit Sub2").OnClick$2(function (s2, e2) {
                    tss.UI.Toast().Information("Clicked: Edit Sub2");
                }), tss.UI.ContextMenuItem("Properties Sub2"), tss.UI.ContextMenuItem("Header Sub2").Header(), tss.UI.ContextMenuItem("Disabled Sub2").Disabled(), tss.UI.ContextMenuItem("Link Sub2").OnClick$2(function (s2, e2) {
                    tss.UI.Toast().Information("Clicked: Link Sub2");
                })]);


                var cmcm = tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem$1(tss.UI.Link("#", "New")).SubMenu(cmsub), tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem$1(tss.UI.Button$1("All").Compact().Link()).OnClick$2(function (_, __) {
                    tss.UI.Toast().Information("Clicked: All");
                }), tss.UI.ContextMenuItem$1(tss.UI.Button$1("Edit").Compact().Link().SetIcon$1(1277)).OnClick$2(function (s2, e2) {
                    tss.UI.Toast().Information("Clicked: Edit");
                }), tss.UI.ContextMenuItem$1(tss.UI.Button$1("Properties").Compact().Link().SetIcon$1(1334)).SubMenu(cmsub2), tss.UI.ContextMenuItem("Header").Header(), tss.UI.ContextMenuItem("Disabled").Disabled(), tss.UI.ContextMenuItem("Link").OnClick$2(function (s2, e2) {
                    tss.UI.Toast().Information("Clicked: Link");
                })]);
                var btn2 = { };
                var btn3 = { };


                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ContextMenuSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ContextualMenus are lists of commands that are based on the context of selection, mouse hover or keyboard focus. They are one of the most effective and highly used command surfaces, and can be used in a variety of places.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use to display commands."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Divide groups of commands with rules."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use selection checks without icons."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Provide submenus for sets of related commands that aren\u2019t as critical as others.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Use them to display content."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Show commands as one large group."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Mix checks and icons."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Create submenus of submenus.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic ContextMenus")), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Standard with Headers").SetContent(tss.UI.Var(tss.Button, tss.UI.Button$1("Open"), btn2).OnClick(function (s, e) {
                    tss.UI.ContextMenu().Items([tss.UI.ContextMenuItem("New").OnClick$2(function (s2, e2) {
                        tss.UI.Toast().Information("Clicked: New");
                    }), tss.UI.ContextMenuItem().Divider(), tss.UI.ContextMenuItem("Edit").OnClick$2(function (s2, e2) {
                        tss.UI.Toast().Information("Clicked: Edit");
                    }), tss.UI.ContextMenuItem("Properties").OnClick$2(function (s2, e2) {
                        tss.UI.Toast().Information("Clicked: Properties");
                    }), tss.UI.ContextMenuItem("Header").Header(), tss.UI.ContextMenuItem("Disabled").Disabled(), tss.UI.ContextMenuItem("Link").OnClick$2(function (s2, e2) {
                        tss.UI.Toast().Information("Clicked: Link");
                    })]).ShowFor$1(btn2.v);
                })), msg]), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Standard with Submenus").SetContent(tss.UI.Var(tss.Button, tss.UI.Button$1("Open"), btn3).OnClick(function (s, e) {
                    cmcm.ShowFor$1(btn3.v);
                })), msg2])])]));
            }
        },
        methods: {
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DatePickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The DatePicker allows users to pick a datetime from a native browser widget."), tss.UI.Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/datetime-local", "Please see here for further information.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic DatePicker")), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Standard").SetContent(tss.UI.DatePicker()), tss.UI.Label$1("With default day of two days in the future").SetContent(tss.UI.DatePicker(System.DateTime.addDays(System.DateTime.getNow(), 2))), tss.UI.Label$1("With step increment of 10").SetContent(tss.UI.DatePicker().SetStep(10)), tss.UI.Label$1(System.String.format("With max of {0}", [System.DateTime.format(to, "D")])).SetContent(tss.UI.DatePicker().SetMax(to)), tss.UI.Label$1(System.String.format("With min of {0}", [System.DateTime.format(from, "D")])).SetContent(tss.UI.DatePicker().SetMin(from)), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.DatePicker().Disabled()), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.DatePicker()), tss.UI.DatePicker().Required(), tss.UI.Label$1("With error message").SetContent(tss.ICVX.IsInvalid(tss.DatePicker, tss.ICVX.Error(tss.DatePicker, tss.UI.DatePicker(), "Error message"))), tss.UI.Label$1("With validation").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), function (dateTimePicker) {
                    return System.DateTime.lte(dateTimePicker.Date, System.DateTime.addMonths(System.DateTime.getNow(), 2)) ? null : "Please choose a date less than 2 months in the future";
                })), tss.UI.Label$1("With validation on type - not in the future").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), tss.Validation.NotInTheFuture)), tss.UI.Label$1("With validation on type - not in the past").SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), tss.Validation.NotInThePast)), tss.UI.Label$1(System.String.format("With validation on type - between {0} and {1}", System.DateTime.format(from, "D"), System.DateTime.format(to, "D"))).SetContent(tss.vX.Validation(tss.DatePicker, tss.UI.DatePicker(), function (dateTimePicker) {
                    return tss.Validation.BetweenRange(dateTimePicker, from, to);
                }))])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DateTimePickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The DateTimePicker allows users to pick a datetime from a native browser widget."), tss.UI.Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/datetime-local", "Please see here for further information.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic DateTimePicker")), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Standard").SetContent(tss.UI.DateTimePicker()), tss.UI.Label$1("With default day of two days in the future").SetContent(tss.UI.DateTimePicker(System.DateTime.addDays(System.DateTime.getNow(), 2))), tss.UI.Label$1("With step increment of 10").SetContent(tss.UI.DateTimePicker().SetStep(10)), tss.UI.Label$1(System.String.format("With max of {0}", [System.DateTime.format(to, "d")])).SetContent(tss.UI.DateTimePicker().SetMax(to)), tss.UI.Label$1(System.String.format("With min of {0}", [System.DateTime.format(from, "d")])).SetContent(tss.UI.DateTimePicker().SetMin(from)), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.DateTimePicker().Disabled()), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.DateTimePicker()), tss.UI.DateTimePicker().Required(), tss.UI.Label$1("With error message").SetContent(tss.ICVX.IsInvalid(tss.DateTimePicker, tss.ICVX.Error(tss.DateTimePicker, tss.UI.DateTimePicker(), "Error message"))), tss.UI.Label$1("With validation").SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), function (dateTimePicker) {
                    return System.DateTime.lte(dateTimePicker.DateTime, System.DateTime.addMonths(System.DateTime.getNow(), 2)) ? null : "Please choose a date less than 2 months in the future";
                })), tss.UI.Label$1("With validation on type - not in the future").SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), tss.Validation.NotInTheFuture$1)), tss.UI.Label$1("With validation on type - not in the past").SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), tss.Validation.NotInThePast$1)), tss.UI.Label$1(System.String.format("With validation on type - between {0} and {1}", System.DateTime.format(from, "d"), System.DateTime.format(to, "d"))).SetContent(tss.vX.Validation(tss.DateTimePicker, tss.UI.DateTimePicker(), function (dateTimePicker) {
                    return tss.Validation.BetweenRange$1(dateTimePicker, from, to);
                }))])]));
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

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DeferSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Use Defer() to render asynchronous components. The asynchronous task is only triggered on the first render of the Defer component")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Number of items:").SetContent(countSlider.OnInput(H5.fn.bind(this, function (s, e) {
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
                            var $s = 0,
                                $t1, 
                                $jff, 
                                $tcs = new H5.TCS(), 
                                $rv, 
                                $ae, 
                                $ab = H5.fn.bind(this, function () {
                                    try {
                                        for (;;) {
                                            $s = H5.STEP([0,1], $s);
                                            switch ($s) {
                                                case 0: {
                                                    $t1 = System.Threading.Tasks.Task.delay(delay.v);
                                                    $s = 1;
                                                    if ($t1.isC()) {
                                                        continue;
                                                    }
                                                    $t1.c($ab);
                                                    return;
                                                }
                                                case 1: {
                                                    $t1.gAR();
                                                    $tcs.sR(tss.ICTX.Children$6(tss.S, tss.ICX.HS(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack())), [tss.ICX.H(tss.Image, tss.ICX.W(tss.Image, tss.UI.Image$1("./assets/img/curiosity-logo.svg"), 40), 40), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.ICX.W(tss.S, tss.UI.VStack(), 50)), 8), [tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), 4), tss.ICX.PT(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), 4), tss.UI.Button$1("Click Me").Primary(), tss.UI.Label$1("Icon:").Inline().SetContent(tss.UI.Icon$2(1810, "fi-br-", "tss-fontsize-large", void 0)), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50)), 4)])]));
                                                    return;
                                                }
                                                default: {
                                                    $tcs.sR(null);
                                                    return;
                                                }
                                            }
                                        }
                                    } catch($ae1) {
                                        $ae = System.Exception.create($ae1);
                                        $tcs.sE($ae);
                                    }
                                }, arguments);

                            $ab();
                            return $tcs.task;
                        });
                    })(this, delay), tss.ICTX.Children$6(tss.S, tss.ICX.HS(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack())), [tss.ICX.H(tss.Image, tss.ICX.W(tss.Image, tss.UI.Image$1(""), 40), 40), tss.ICTX.Children$6(tss.S, tss.ICX.PL(tss.S, tss.ICX.Grow(tss.S, tss.ICX.W(tss.S, tss.UI.VStack(), 50)), 8), [tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), 4), tss.ICX.PT(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), 4), tss.UI.Button$1("Click Me").Primary(), tss.UI.Label$1("Icon:").Inline().SetContent(tss.UI.Icon$2(1810, "fi-br-", "tss-fontsize-large", void 0)), tss.ICX.PT(tss.txt, tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50)), 4)])]).Skeleton())]));
                }
            },
            Render: function () {
                return this.content.tss$IC$Render();
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DetailsListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("DetailsList is a derivative of the List component. It is a robust way to display an information rich collection of items. It can support powerful ways to aid a user in finding content with sorting, grouping and filtering.  Lists are a great way to handle large amounts of content, but poorly designed Lists can be difficult to parse."), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("Use a DetailsList when density of information is critical. Lists can support single and multiple selection, as well as drag and drop and marquee selection. They are composed of a column header, which contains the metadata fields which are attached to the list items, and provide the ability to sort, filter and even group the list. List items are composed of selection, icon, and name columns at minimum. One can also include other columns such as Date Modified, or any other metadata field associated with the collection. Place the most important columns from left to right for ease of recall and comparison."), tss.usX.px$1(16)), tss.UI.TextBlock("DetailsList is classically used to display files, but is also used to render custom lists that can be purely metadata. Avoid using file type icon overlays to denote status of a file as it can make the entire icon unclear. Be sure to leave ample width for each column\u2019s data. If there are multiple lines of text in a column, consider the variable row height variant.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.Stack().Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use them to display content."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Provide useful columns of metadata."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Display columns in order of importance left to right or right to left depending on the standards of the culture."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Give columns ample default width to display information.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Use them to display commands or settings."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Overload the view with too many columns that require excessive horizontal scrolling."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Make columns so narrow that it truncates the information in typical cases.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Details List With Textual Rows")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Shrink(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1440), tss.usX.px$1(32), true, "FileIcon", void 0), tss.UI.DetailsListColumn("File Name", tss.usX.px$1(350), true, true, "FileName", void 0), tss.UI.DetailsListColumn("Date Modified", tss.usX.px$1(170), false, true, "DateModified", void 0), tss.UI.DetailsListColumn("Modified By", tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn("File Size", tss.usX.px$1(120), false, true, "FileSize", void 0)]), tss.usX.px$1(500)).WithListItems(this.GetDetailsListItems()).SortedBy("FileName")), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Details List With Component Rows and responsive width")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Width(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1440), tss.usX.px$1(80), true, "FileIcon", void 0), tss.UI.DetailsListColumn("File Name", tss.usX.percent$1(40), true, true, "FileName", void 0), tss.UI.DetailsListColumn$1("Date Modified", tss.usX.percent$1(20), tss.usX.px$1(150), false, true, "DateModified", void 0), tss.UI.DetailsListColumn$1("Modified By", tss.usX.percent$1(20), tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn$1("File Size", tss.usX.percent$1(20), tss.usX.px$1(100), false, true, "FileSize", void 0)]), tss.usX.px$1(500)), tss.usX.percent$1(100)).WithListItems(this.GetDetailsListItems()).SortedBy("FileName"), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Details List With Component Rows")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents, [tss.UI.IconColumn(tss.UI.Icon$2(106), tss.usX.px$1(32), true, "Icon", void 0), tss.UI.DetailsListColumn("CheckBox", tss.usX.px$1(120), false, false, void 0, void 0), tss.UI.DetailsListColumn("Name", tss.usX.px$1(250), true, false, void 0, void 0), tss.UI.DetailsListColumn("Button", tss.usX.px$1(150), false, false, void 0, void 0), tss.UI.DetailsListColumn("ChoiceGroup", tss.usX.px$1(400), false, false, void 0, void 0), tss.UI.DetailsListColumn("Dropdown", tss.usX.px$1(250), false, false, void 0, void 0), tss.UI.DetailsListColumn("Toggle", tss.usX.px$1(100), false, false, void 0, void 0)]).Compact(), tss.usX.px$1(500)).WithListItems(this.GetComponentDetailsListItems()).SortedBy("Name"), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Details List With Textual Rows and paginated content")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Shrink(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1440), tss.usX.px$1(32), true, "FileIcon", void 0), tss.UI.DetailsListColumn("File Name", tss.usX.px$1(350), true, true, "FileName", void 0), tss.UI.DetailsListColumn("Date Modified", tss.usX.px$1(170), false, true, "DateModified", void 0), tss.UI.DetailsListColumn("Modified By", tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn("File Size", tss.usX.px$1(120), false, true, "FileSize", void 0)]), tss.usX.px$1(500)).WithListItems(this.GetDetailsListItems(0, page)).WithPaginatedItems(H5.fn.bind(this, function () {
                    var $s = 0,
                        $t1, 
                        $tr1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            page = (page + 1) | 0;
                                            tss.Router.ReplaceQueryParameters(function (parameters) {
                                                return parameters.With("page", H5.toString(page));
                                            });
                                            $t1 = this.GetDetailsListItemsAsync(page, 1);
                                            $s = 1;
                                            if ($t1.isC()) {
                                                continue;
                                            }
                                            $t1.c($ab);
                                            return;
                                        }
                                        case 1: {
                                            $tr1 = $t1.gAR();
                                            $tcs.sR($tr1);
                                            return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                })).SortedBy("FileName")), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Details List With Empty List Message")), tss.usX.px$1(16)), tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem), tss.UI.DetailsList(Tesserae.Tests.Samples.DetailsListSampleFileItem, [tss.UI.IconColumn(tss.UI.Icon$2(1440), tss.usX.px$1(32), true, "FileIcon", void 0), tss.UI.DetailsListColumn("File Name", tss.usX.px$1(350), true, true, "FileName", void 0), tss.UI.DetailsListColumn("Date Modified", tss.usX.px$1(170), false, true, "DateModified", void 0), tss.UI.DetailsListColumn("Modified By", tss.usX.px$1(150), false, true, "ModifiedBy", void 0), tss.UI.DetailsListColumn("File Size", tss.usX.px$1(120), false, true, "FileSize", void 0)]).Compact().WithEmptyMessage(function () {
                    return tss.ICX.HeightStretch(tss.BackgroundArea, tss.ICX.WidthStretch(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("Empty list"), tss.usX.px$1(16))))));
                }), tss.usX.px$1(500)).WithListItems(System.Array.init(0, null, Tesserae.Tests.Samples.DetailsListSampleFileItem)).SortedBy("Name")]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetDetailsListItemsAsync: function (start, count) {
                var $s = 0,
                    $t1, 
                    $jff, 
                    $tcs = new H5.TCS(), 
                    $rv, 
                    $ae, 
                    $ab = H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                $s = H5.STEP([0,1], $s);
                                switch ($s) {
                                    case 0: {
                                        if (start === void 0) { start = 1; }
                                        if (count === void 0) { count = 100; }
                                        $t1 = System.Threading.Tasks.Task.delay(1000);
                                        $s = 1;
                                        if ($t1.isC()) {
                                            continue;
                                        }
                                        $t1.c($ab);
                                        return;
                                    }
                                    case 1: {
                                        $t1.gAR();
                                        $tcs.sR(this.GetDetailsListItems(start, count));
                                        return;
                                    }
                                    default: {
                                        $tcs.sR(null);
                                        return;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            $tcs.sE($ae);
                        }
                    }, arguments);

                $ab();
                return $tcs.task;
            },
            GetDetailsListItems: function (start, count) {
                if (start === void 0) { start = 1; }
                if (count === void 0) { count = 100; }
                return System.Linq.Enumerable.range(start, count).selectMany(function (number) {
                    return function (_o1) {
                            _o1.add(new Tesserae.Tests.Samples.DetailsListSampleFileItem(1474, (System.String.format("Interesting File Name {0}, quite long as you can see. In fact, let's make it ", [H5.box(number, System.Int32)]) || "") + "longer to see how the padding looks.", System.DateTime.addDays(System.DateTime.getToday(), ((-number) | 0)), "Dale Cooper", number + 0.7));
                            _o1.add(new Tesserae.Tests.Samples.DetailsListSampleFileItem(1453, System.String.format("File Name {0}", [H5.box(number, System.Int32)]), System.DateTime.addDays(System.DateTime.getToday(), ((-number) | 0)), "Rusty", number + 0.1));
                            _o1.add(new Tesserae.Tests.Samples.DetailsListSampleFileItem(1465, System.String.format("File Name {0}", [H5.box(number, System.Int32)]), System.DateTime.addDays(System.DateTime.getToday(), ((-number) | 0)), "Cole", number + 0.5));
                            return _o1;
                        }(new (System.Collections.Generic.List$1(Tesserae.Tests.Samples.DetailsListSampleFileItem)).ctor());
                }).ToArray(Tesserae.Tests.Samples.DetailsListSampleFileItem);
            },
            GetComponentDetailsListItems: function () {
                return System.Linq.Enumerable.range(1, 100).selectMany(function (number) {
                    return function (_o1) {
                            _o1.add(new Tesserae.Tests.Samples.DetailsListSampleItemWithComponents().WithIcon(3194).WithCheckBox(tss.UI.CheckBox$1("CheckBox")).WithName("Component Details List Item").WithButton(tss.UI.Button$1().SetText("Primary").Primary().OnClick(function (s, e) {
                                alert("Clicked!");
                            })).WithChoiceGroup(tss.UI.ChoiceGroup().Horizontal().Choices([tss.UI.Choice("Option A"), tss.UI.Choice("Option B").Disabled(), tss.UI.Choice("Option C")])).WithDropdown(tss.UI.Dropdown().Multi().Items$1([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1"), tss.UI.DropdownItem$1("1-2").Selected(), tss.UI.DropdownItem$1("1-3"), tss.UI.DropdownItem$1("1-4").Disabled(), tss.UI.DropdownItem$1("1-5"), tss.UI.DropdownItem$1("2-1"), tss.UI.DropdownItem$1("2-2"), tss.UI.DropdownItem$1("2-3"), tss.UI.DropdownItem$1("2-4").Selected(), tss.UI.DropdownItem$1("2-5")])).WithToggle(tss.UI.Toggle()));
                            return _o1;
                        }(new (System.Collections.Generic.List$1(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents)).ctor());
                }).ToArray(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents);
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
                var dialog = tss.UI.Dialog("Lorem Ipsum");
                var response = tss.UI.TextBlock$1();

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DialogSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Dialogs are temporary, modal UI overlay that generally provide contextual app information or require user confirmation/input. In most cases, Dialogs block interactions with the web page or application until being explicitly dismissed, and often request action from the user. They are primarily used for lightweight creation or edit tasks, and simple management tasks."), tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock("Best Practices"))])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use Dialogs for quick, actionable interactions, such as making a choice or needing the user to provide information."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("When possible, try a non-blocking Dialog before resorting to a blocking Dialog."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Only include information needed to help users make a decision."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Button text should reflect the actions available to the user (e.g. save, delete)."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Validate that the user's entries are acceptable before closing the Dialog. Show an inline validation error near the field they must correct.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t overuse Modals. In some cases they can be perceived as interrupting workflow, and too many can be a bad user experience."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Avoid \"Are you sure ?\" or confirmation Dialogs unless the user is making an irreversible or destructive choice."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Do not use a blocking Dialog unless absolutely necessary because they are very disruptive."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t have long sentences or complicated choices."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Avoid generic button labels like \"Ok\" if you can be more specific about the action a user is about to complete."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't dismiss the Dialog if underlying problem is not fixed. Don't put the user back into a broken/error state."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't provide the user with more than 3 buttons.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Dialog").OnClick(function (c, ev) {
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
                },
                GetAsync: function (url) {
                    var $s = 0,
                        $t1, 
                        $tr1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        xmlHttp, 
                        tcs, 
                        tcsTask, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1,2,3,4], $s);
                                    switch ($s) {
                                        case 0: {
                                            xmlHttp = new (XMLHttpRequest)();
                                            xmlHttp.open("GET", url, true);

                                            xmlHttp.setRequestHeader("Access-Control-Allow-Origin", "*");

                                            tcs = new System.Threading.Tasks.TaskCompletionSource();

                                            xmlHttp.onreadystatechange = function (e) {
                                                if (xmlHttp.readyState === 0) {
                                                    tcs.setException(new System.Exception("Request aborted"));
                                                } else if (xmlHttp.readyState === 4) {
                                                    if (xmlHttp.status === 200 || xmlHttp.status === 201 || xmlHttp.status === 304) {
                                                        tcs.setResult(xmlHttp.responseText);
                                                    } else {
                                                        tcs.setException(new System.Exception("Request failed"));
                                                    }
                                                }
                                            };

                                            xmlHttp.send();

                                            tcsTask = tcs.task;
                                            $s = 1;
                                            continue;
                                        }
                                        case 1: {
                                            if ( true ) {
                                                $s = 2;
                                                continue;
                                            } 
                                            $s = 4;
                                            continue;
                                        }
                                        case 2: {
                                            $t1 = System.Threading.Tasks.Task.whenAny(tcsTask, System.Threading.Tasks.Task.delay(150));
                                            $s = 3;
                                            if ($t1.isC()) {
                                                continue;
                                            }
                                            $t1.c($ab);
                                            return;
                                        }
                                        case 3: {
                                            $tr1 = $t1.gAR();
                                            if (tcsTask.isCompleted()) {
                                                if (tcsTask.isFaulted()) {
                                                    throw tcsTask.getException();
                                                }
                                                $tcs.sR(tcsTask.getResult());
                                                return;
                                            }

                                            $s = 1;
                                            continue;
                                        }
                                        case 4: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
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
                var dropdownForValidationExample = tss.UI.Dropdown();

                dropdownForValidationExample.Attach(function (dd) {
                    var $t;
                    if (dd.SelectedItems.length !== 1 || !H5.rE(($t = dd.SelectedItems)[System.Array.index(0, $t)].Text, "1-1")) {
                        dd.IsInvalid = true;
                        dd.Error = "Some error happens, need 1-1";
                    } else {
                        dd.IsInvalid = false;
                    }
                });

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("DropdownSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Dropdown is a list in which the selected item is always visible, and the others are visible on demand by clicking a drop-down button. They are used to simplify the design and make a choice within the UI. When closed, only the selected item is visible. When users click the drop-down button, all the options become visible. To change the value, users open the list and click another value or use the arrow keys (up and down) to select a new value.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use a Dropdown when there are multiple choices that can be collapsed under one title. Or if the list of items is long or when space is constrained."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use shortened statements or single words as options."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use a Dropdown when the selected option is more important than the alternatives (in contrast to radio buttons where all the choices are visible putting more emphasis on the other options).")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use if there are less than 7 options, use a ChoiceGroup instead.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic Dropdowns")), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Standard").SetContent(tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1("1-1").Selected(), tss.UI.DropdownItem$1("1-2")]))]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Standard with Headers").SetContent(tss.UI.Dropdown().Items$1([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1"), tss.UI.DropdownItem$1("1-2"), tss.UI.DropdownItem$1("1-3"), tss.UI.DropdownItem$1("1-4").Disabled(), tss.UI.DropdownItem$1("1-5"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("2-1"), tss.UI.DropdownItem$1("2-2"), tss.UI.DropdownItem$1("2-3").Selected(), tss.UI.DropdownItem$1("2-4"), tss.UI.DropdownItem$1("2-5")])), tss.UI.Label$1("Multi-select with custom on-selected text").SetContent(tss.UI.Dropdown().Multi().Items$1([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1", "H1-1-1"), tss.UI.DropdownItem$1("1-2", "H1-1-2").Selected(), tss.UI.DropdownItem$1("1-3", "H1-1-3"), tss.UI.DropdownItem$1("1-4", "H1-1-4").Disabled(), tss.UI.DropdownItem$1("1-5", "H1-1-5"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("2-1", "H2-2-1"), tss.UI.DropdownItem$1("2-2", "H2-2-2"), tss.UI.DropdownItem$1("2-3", "H2-2-3"), tss.UI.DropdownItem$1("2-4", "H2-2-4").Selected(), tss.UI.DropdownItem$1("2-5", "H2-2-5")])), tss.UI.Label$1("Validation").SetContent(dropdownForValidationExample.Items$1([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1").Selected(), tss.UI.DropdownItem$1("1-2"), tss.UI.DropdownItem$1("1-3"), tss.UI.DropdownItem$1("1-4").Disabled(), tss.UI.DropdownItem$1("1-5"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("2-1"), tss.UI.DropdownItem$1("2-2"), tss.UI.DropdownItem$1("2-3"), tss.UI.DropdownItem$1("2-4"), tss.UI.DropdownItem$1("2-5")])), tss.UI.Label$1("Disabled").SetContent(tss.UI.Dropdown().Disabled().Items$1([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1").Selected(), tss.UI.DropdownItem$1("1-2"), tss.UI.DropdownItem$1("1-3"), tss.UI.DropdownItem$1("1-4").Disabled(), tss.UI.DropdownItem$1("1-5"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("2-1"), tss.UI.DropdownItem$1("2-2"), tss.UI.DropdownItem$1("2-3"), tss.UI.DropdownItem$1("2-4"), tss.UI.DropdownItem$1("2-5")])), tss.UI.Label$1("Required").SetContent(tss.UI.Dropdown().Required().Items$1([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1").Selected(), tss.UI.DropdownItem$1("1-2"), tss.UI.DropdownItem$1("1-3"), tss.UI.DropdownItem$1("1-4").Disabled(), tss.UI.DropdownItem$1("1-5"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("2-1"), tss.UI.DropdownItem$1("2-2"), tss.UI.DropdownItem$1("2-3"), tss.UI.DropdownItem$1("2-4"), tss.UI.DropdownItem$1("2-5")])), tss.UI.Label$1("No available items").SetContent(tss.UI.Dropdown().Items$1(System.Array.init(0, null, tss.Dropdown.Item))), tss.UI.Label$1("No available items with custom message").SetContent(tss.UI.Dropdown$2("There's nothing to select, sorry!").Items$1(System.Array.init(0, null, tss.Dropdown.Item))), tss.UI.Label$1("Async 5 seconds delay (starts loading immediately)").SetContent(Tesserae.Tests.Samples.DropdownSample.StartLoadingAsyncDataImmediately(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetItemsAsync)))), tss.UI.Label$1("Async 5 seconds delay (will start loading when dropdown is opened)").SetContent(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetItemsAsync))), tss.UI.Label$1("Async 5 seconds delay that returns no items (starts loading immediately)").SetContent(Tesserae.Tests.Samples.DropdownSample.StartLoadingAsyncDataImmediately(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetZeroItemsAsync)))), tss.UI.Label$1("Async wait Google.com (need CORS)").SetContent(tss.UI.Dropdown().Items(H5.fn.cacheBind(this, this.GetGoogleItemsAsync)))])]));
            }
        },
        methods: {
            GetItemsAsync: function () {
                var $s = 0,
                    $t1, 
                    $jff, 
                    $tcs = new H5.TCS(), 
                    $rv, 
                    $ae, 
                    $ab = H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                $s = H5.STEP([0,1], $s);
                                switch ($s) {
                                    case 0: {
                                        $t1 = System.Threading.Tasks.Task.delay(5000);
                                        $s = 1;
                                        if ($t1.isC()) {
                                            continue;
                                        }
                                        $t1.c($ab);
                                        return;
                                    }
                                    case 1: {
                                        $t1.gAR();
                                        $tcs.sR(System.Array.init([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1"), tss.UI.DropdownItem$1("1-2"), tss.UI.DropdownItem$1("1-3"), tss.UI.DropdownItem$1("1-4").Disabled(), tss.UI.DropdownItem$1("1-5"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("2-1"), tss.UI.DropdownItem$1("2-2"), tss.UI.DropdownItem$1("2-3"), tss.UI.DropdownItem$1("2-4"), tss.UI.DropdownItem$1("2-5")], tss.Dropdown.Item));
                                        return;
                                    }
                                    default: {
                                        $tcs.sR(null);
                                        return;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            $tcs.sE($ae);
                        }
                    }, arguments);

                $ab();
                return $tcs.task;
            },
            GetZeroItemsAsync: function () {
                var $s = 0,
                    $t1, 
                    $jff, 
                    $tcs = new H5.TCS(), 
                    $rv, 
                    $ae, 
                    $ab = H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                $s = H5.STEP([0,1], $s);
                                switch ($s) {
                                    case 0: {
                                        $t1 = System.Threading.Tasks.Task.delay(5000);
                                        $s = 1;
                                        if ($t1.isC()) {
                                            continue;
                                        }
                                        $t1.c($ab);
                                        return;
                                    }
                                    case 1: {
                                        $t1.gAR();
                                        $tcs.sR(System.Array.init(0, null, tss.Dropdown.Item));
                                        return;
                                    }
                                    default: {
                                        $tcs.sR(null);
                                        return;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            $tcs.sE($ae);
                        }
                    }, arguments);

                $ab();
                return $tcs.task;
            },
            GetGoogleItemsAsync: function () {
                var $s = 0,
                    $t1, 
                    $tr1, 
                    $t2, 
                    $jff, 
                    $tcs = new H5.TCS(), 
                    $rv, 
                    $ae, 
                    $ae1, 
                    $ab = H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                $s = H5.STEP([1,2,3,4,5], $s);
                                switch ($s) {

                                    case 1: {
                                        $t1 = Tesserae.Tests.Samples.DropdownSample.GetAsync("http://google.com");
                                        $s = 2;
                                        if ($t1.isC()) {
                                            continue;
                                        }
                                        $t1.c($ab);
                                        return;
                                    }
                                    case 2: {
                                        $tr1 = $t1.gAR();
                                        $s = 5;
                                        continue;
                                    }
                                    case 3: {
                                        $t2 = System.Threading.Tasks.Task.delay(1000);
                                        $s = 4;
                                        if ($t2.isC()) {
                                            continue;
                                        }
                                        $t2.c($ab);
                                        return;
                                    }
                                    case 4: {
                                        $t2.gAR();
                                        $ae = null;
                                        $s = 5;
                                        continue;
                                    }
                                    case 5: {
                                        $tcs.sR(System.Array.init([tss.UI.DropdownItem$1("Header 1").Header(), tss.UI.DropdownItem$1("1-1"), tss.UI.DropdownItem$1("1-2"), tss.UI.DropdownItem$1("1-3"), tss.UI.DropdownItem$1("1-4").Disabled(), tss.UI.DropdownItem$1("1-5"), tss.UI.DropdownItem().Divider(), tss.UI.DropdownItem$1("Header 2").Header(), tss.UI.DropdownItem$1("2-1"), tss.UI.DropdownItem$1("2-2"), tss.UI.DropdownItem$1("2-3"), tss.UI.DropdownItem$1("2-4"), tss.UI.DropdownItem$1("2-5")], tss.Dropdown.Item));
                                        return;
                                    }
                                    default: {
                                        $tcs.sR(null);
                                        return;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            if ( $s >= 1 && $s <= 2 ) {
                                $s = 3;
                                $ab();
                                return;
                            }
                            $tcs.sE($ae);
                        }
                    }, arguments);

                $ab();
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("EditableLabelSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Use for showing information that can be edited by users.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.Stack().Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use anywhere information can be edited easily by users.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t forget to register a OnSave() callback")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Label")), tss.UI.EditableLabel("You can click to edit me"), tss.ITFX.Large(tss.EditableLabel, tss.UI.EditableLabel("You can also change the font-size")), tss.ITFX.Bold(tss.EditableLabel, tss.ITFX.Large(tss.EditableLabel, tss.UI.EditableLabel("and weight as a normal label"))), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Text Area")), tss.ICX.Width(tss.EditableArea, tss.UI.EditableArea("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\nUt enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.\nDuis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.\nExcepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.px$1(300))]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("EmojiSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Tesserae integrates the Emoji.css stylesheet as part of the package, with an auto-generated strongly typed enum for them.")])), tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle(System.String.format("enum {0}:", ["Emoji"])), tss.UI.SearchableList(Tesserae.Tests.Samples.EmojiSample.IconItem, ($t = Tesserae.Tests.Samples.EmojiSample.IconItem, System.Linq.Enumerable.from(this.GetAllIcons(), $t).ToArray($t)), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)])])), true, false, "");
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("FileSelectorAndDropAreaSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Use the file selector to allow users to select a single file. Use the file dropdown area to allow drag-and-drop for one or multiple files.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Filter files by supported types"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Provide a message for the file drop area"), Tesserae.Tests.Samples.SamplesHelper.SampleDo(System.String.format("Attach the {0} event handler", ["OnFileDropped"]))]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("TODO")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("File Selector"), tss.UI.Label$1("Selected file size: ").Inline().SetContent(tss.UI.Var(tss.txt, tss.UI.TextBlock(""), size)), tss.UI.FileSelector().OnFileSelected(function (fs, e) {
                    size.v.Text = (H5.toString(fs.SelectedFile.size) || "") + " bytes";
                }), tss.UI.FileSelector().SetPlaceholder("You must select a zip file").Required().SetAccepts(".zip").OnFileSelected(function (fs, e) {
                    size.v.Text = (H5.toString(fs.SelectedFile.size) || "") + " bytes";
                }), tss.UI.FileSelector().SetPlaceholder("Please select any image").SetAccepts("image/*").OnFileSelected(function (fs, e) {
                    size.v.Text = (H5.toString(fs.SelectedFile.size) || "") + " bytes";
                }), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("File Drop Area"), tss.UI.Label$1("Dropped Files: ").SetContent(tss.UI.Var(tss.S, tss.UI.Stack(), droppedFiles)), tss.UI.FileDropArea().OnFileDropped(function (s, e) {
                    droppedFiles.v.Add(tss.ITFX.Small(tss.txt, tss.UI.TextBlock(e.name)));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("FloatSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Floats are used to create absolute-positioned overlays within other containers")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()).Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Make sure you're not covering anything else under your container")]), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't forget to add .Relative() to the parent Stack or Grid where you place a Float component!")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Possible Positions")), tss.ICTX.Children$6(tss.S, tss.ICX.Height(tss.S, tss.ICX.WS(tss.S, tss.UI.Stack().Relative().Horizontal()), tss.usX.px$1(400)), [tss.UI.Float(tss.UI.Button$1("TopLeft"), "tss-float-topleft"), tss.UI.Float(tss.UI.Button$1("TopMiddle"), "tss-float-topmiddle"), tss.UI.Float(tss.UI.Button$1("TopRight"), "tss-float-topright"), tss.UI.Float(tss.UI.Button$1("LeftCenter"), "tss-float-leftcenter"), tss.UI.Float(tss.UI.Button$1("Center"), "tss-float-center"), tss.UI.Float(tss.UI.Button$1("RightCenter"), "tss-float-rightcenter"), tss.UI.Float(tss.UI.Button$1("BottomLeft"), "tss-float-bottomleft"), tss.UI.Float(tss.UI.Button$1("BottonMiddle"), "tss-float-bottonmiddle"), tss.UI.Float(tss.UI.Button$1("BottomRight"), "tss-float-bottomright")])]));
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
                    var GetNeighbors = null;
                    var Grow = null;
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
                        var color = "";

                        switch (state) {
                            case 0: 
                                color = tss.UI.Theme.Default.Background;
                                break;
                            case 1: 
                                color = tss.UI.Theme.Default.Foreground;
                                break;
                        }
                        tss.ISX.Background(tss.Button, btn, color);
                    }, System.Array.init([tss.usX.px$1(128), tss.usX.px$1(24)], tss.us), tss.usX.px$1(24));
                    Grow = function () {
                        var $t, $t1, $t2, $t3;
                        if (grid.IsDragging) {
                            return;
                        }
                        if (isPaused) {
                            return;
                        }
                        var previous = grid.GetState();
                        var cells = grid.GetState();
                        for (var i = 0; i < height; i = (i + 1) | 0) {
                            for (var j = 0; j < width; j = (j + 1) | 0) {
                                var numOfAliveNeighbors = GetNeighbors(previous, i, j);
                                if (($t = cells[System.Array.index(i, cells)])[System.Array.index(j, $t)] === 1) {
                                    if (numOfAliveNeighbors < 2) {
                                        ($t1 = cells[System.Array.index(i, cells)])[System.Array.index(j, $t1)] = 0;
                                    }

                                    if (numOfAliveNeighbors > 3) {
                                        ($t2 = cells[System.Array.index(i, cells)])[System.Array.index(j, $t2)] = 0;
                                    }
                                } else {
                                    if (numOfAliveNeighbors === 3) {
                                        ($t3 = cells[System.Array.index(i, cells)])[System.Array.index(j, $t3)] = 1;
                                    }
                                }
                            }
                        }

                        grid.SetState(cells);
                    };
                    GetNeighbors = function (cells, x, y) {
                        var $t;
                        var NumOfAliveNeighbors = 0;
                        for (var i = (x - 1) | 0; i < ((x + 2) | 0); i = (i + 1) | 0) {
                            for (var j = (y - 1) | 0; j < ((y + 2) | 0); j = (j + 1) | 0) {
                                if (i < 0 || j < 0 || i >= height || j >= width || i === x && j === y) {
                                    continue;
                                }

                                if (($t = cells[System.Array.index(i, cells)])[System.Array.index(j, $t)] === 1) {
                                    NumOfAliveNeighbors = (NumOfAliveNeighbors + 1) | 0;
                                }
                            }
                        }

                        return NumOfAliveNeighbors;
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

                    var btnReset = tss.UI.Button$1("Reset").SetIcon$1(380).OnClick$1(function () {
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

                    var btnPause = tss.UI.Button$1("Pause").SetIcon$1(2463);

                    btnPause.OnClick$1(function () {
                        isPaused = !isPaused;
                        btnPause.SetIcon$1(isPaused ? 2613 : 2463);
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("GridPickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("This component let you select states on a grid")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.HorizontalSeparator("Daytime Example").Left(), picker, tss.UI.HorizontalSeparator("Hour Example").Left(), hourPicker, tss.UI.HorizontalSeparator("Game of Life").Left(), Tesserae.Tests.Samples.GridPickerSample.GetGameOfLifeSample()]));
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
                grid.Gap(tss.usX.px$1(6));
                var btn = tss.ICX.WS(tss.Button, tss.UI.Button$1().SetText("this")).Primary();
                tss.ICX.GridColumnStretch(tss.Button, btn);
                tss.ICX.GridRow(tss.Button, btn, 1, 2);
                grid.Add(btn);
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));
                grid.Add(tss.UI.Button$1().SetText("other"));


                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("GridSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("This component let you add items to a grid")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.HorizontalSeparator("Daytime Example").Left(), grid]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("HorizontalSeparatorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A separator visually separates content into groups."), tss.UI.TextBlock("You can render content in the separator by specifying the component's children. The component's children can be plain text or a component like Icon. The content is center-aligned by default.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Explain what is the group this separator introduces"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Be short and concise.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Use long group names")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.HorizontalSeparator("Center"), tss.UI.HorizontalSeparator("Left").Left(), tss.UI.HorizontalSeparator("Right").Right(), Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Custom Separators"), tss.UI.HorizontalSeparator$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.PaddingRight(tss.Icon, tss.ICX.AlignCenter(tss.Icon, tss.UI.Icon$2(2596)), tss.usX.px$1(8)), tss.ICX.AlignCenter(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Custom Center"))))])).Primary(), tss.UI.HorizontalSeparator$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.PaddingRight(tss.Icon, tss.ICX.AlignCenter(tss.Icon, tss.UI.Icon$2(2596)), tss.usX.px$1(8)), tss.ICX.AlignCenter(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Custom Left"))))])).Primary().Left(), tss.UI.HorizontalSeparator$1(tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.PaddingRight(tss.Icon, tss.ICX.AlignCenter(tss.Icon, tss.UI.Icon$2(2596)), tss.usX.px$1(8)), tss.ICX.AlignCenter(tss.txt, tss.ITFX.MediumPlus(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Custom Right"))))])).Primary().Right()]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("InfiniteScrollingListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("List provides a base component for rendering paginates sets of items. It is agnostic of the tile component used, and selection management. These concerns can be layered separately."), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("Performance is adequate for smaller lists, for large number of items use VirtualizedList."), tss.usX.px$1(16))])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic List with VisibilitySensor")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.InfiniteScrollingList, tss.ICX.Height(tss.InfiniteScrollingList, tss.UI.InfiniteScrollingList$2(this.GetSomeItems(20, 0, " initial"), H5.fn.bind(this, function () {
                    var $s = 0,
                        $t1, 
                        $tr1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            $t1 = this.GetSomeItemsAsync(20, H5.identity(page, ((page = (page + 1) | 0))));
                                                $s = 1;
                                                if ($t1.isC()) {
                                                    continue;
                                                }
                                                $t1.c($ab);
                                                return;
                                        }
                                        case 1: {
                                            $tr1 = $t1.gAR();
                                            $tcs.sR($tr1);
                                                return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                })), tss.usX.px$1(500)), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic Grid List with VisibilitySensor")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.InfiniteScrollingList, tss.ICX.Height(tss.InfiniteScrollingList, tss.UI.InfiniteScrollingList$2(this.GetSomeItems(20, 0, " initial"), H5.fn.bind(this, function () {
                    var $s = 0,
                        $t1, 
                        $tr1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            $t1 = this.GetSomeItemsAsync(20, H5.identity(pageGrid, ((pageGrid = (pageGrid + 1) | 0))));
                                                $s = 1;
                                                if ($t1.isC()) {
                                                    continue;
                                                }
                                                $t1.c($ab);
                                                return;
                                        }
                                        case 1: {
                                            $tr1 = $t1.gAR();
                                            $tcs.sR($tr1);
                                                return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                }), [tss.usX.percent$1(33), tss.usX.percent$1(33), tss.usX.percent$1(34)]), tss.usX.px$1(500)), tss.usX.px$1(32))]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetSomeItems: function (count, page, txt) {
                if (page === void 0) { page = -1; }
                if (txt === void 0) { txt = ""; }
                var pageString = page > 0 ? H5.toString(page) : "";

                return System.Linq.Enumerable.range(1, count).select(function (number) {
                    var card = tss.ICX.MinWidth(tss.Card, tss.UI.Card(tss.txtX.NonSelectable(tss.txt, tss.UI.TextBlock((System.String.format("Lorem Ipsum {0} / {1}", pageString, H5.box(number, System.Int32)) || "") + (txt || "")))), tss.usX.px$1(200));
                    return card;
                }).ToArray(tss.Card);
            },
            GetSomeItemsAsync: function (count, page, txt) {
                var $s = 0,
                    $t1, 
                    $jff, 
                    $tcs = new H5.TCS(), 
                    $rv, 
                    $ae, 
                    $ab = H5.fn.bind(this, function () {
                        try {
                            for (;;) {
                                $s = H5.STEP([0,1], $s);
                                switch ($s) {
                                    case 0: {
                                        if (page === void 0) { page = -1; }
                                        if (txt === void 0) { txt = ""; }
                                        $t1 = System.Threading.Tasks.Task.delay(200);
                                        $s = 1;
                                        if ($t1.isC()) {
                                            continue;
                                        }
                                        $t1.c($ab);
                                        return;
                                    }
                                    case 1: {
                                        $t1.gAR();
                                        $tcs.sR(this.GetSomeItems(count, page, txt));
                                        return;
                                    }
                                    default: {
                                        $tcs.sR(null);
                                        return;
                                    }
                                }
                            }
                        } catch($ae1) {
                            $ae = System.Exception.create($ae1);
                            $tcs.sE($ae);
                        }
                    }, arguments);

                $ab();
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
                    obsList.AddRange(this.GetSomeItems(20));
                    v.Reset();
                    obsList.add(v);
                }));

                obsList.AddRange(this.GetSomeItems(10));
                obsList.add(vs);

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ItemsListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("List provides a base component for rendering small sets of items. It is agnostic of the tile component used, and selection management. These concerns can be layered separately."), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("Performance is adequate for smaller lists, for large number of items use VirtualizedList."), tss.usX.px$1(16))])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic List")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(10)), tss.usX.px$1(16)), tss.usX.px$1(500)), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic List with columns")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(100), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)]), tss.usX.px$1(500)), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic List with VisibilitySensor")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.ItemsList, tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList$1(obsList, [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)]), tss.usX.px$1(500)), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic List with Empty List Message ")), tss.usX.px$1(16)), tss.ICX.Height(tss.ItemsList, tss.UI.ItemsList(System.Array.init(0, null, tss.IC), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)]).WithEmptyMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HeightStretch(tss.BackgroundArea, tss.ICX.WidthStretch(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("Empty list"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.usX.px$1(500))]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetSomeItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (number) {
                    return tss.ICX.MinWidth(tss.Card, tss.UI.Card(tss.txtX.NonSelectable(tss.txt, tss.UI.TextBlock(System.String.format("Lorem Ipsum {0}", [H5.box(number, System.Int32)])))), tss.usX.px$1(200));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("LabelSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Labels give a name or title to a component or group of components. Labels should be in close proximity to the component or group they are paired with. Some components, such as TextField, Dropdown, or Toggle, already have Labels incorporated, but other components may optionally add a Label if it helps inform the user of the component\u2019s purpose.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use sentence casing, e.g. \u201cFirst name\u201d."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Be short and concise."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("When adding a Label to components, use the text as a noun or short noun phrase.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use Labels as instructional text, e.g. \u201cClick to get started\u201d."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Don\u2019t use full sentences or complex punctuation (colons, semicolons, etc.).")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Label$1("I'm Label"), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("I'm a disabled Label")), tss.txtX.Required(tss.Label, tss.UI.Label$1("I'm a required Label")), tss.txtX.Primary(tss.Label, tss.UI.Label$1("I'm a primary Label")), tss.txtX.Secondary(tss.Label, tss.UI.Label$1("I'm a secondary Label")), tss.ITFX.Tiny(tss.Label, tss.ITFX.Regular(tss.Label, tss.UI.Label$1("I'm a tiny Label"))), tss.UI.Label$1("A Label for An Input").SetContent(tss.UI.TextBox$1()), tss.ICX.PaddingBottom(tss.txt, tss.ICX.PaddingTop(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Inline without auto-width")), tss.usX.px$1(16)), tss.usX.px$1(8)), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Lbl").Inline().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Label").Inline().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Bigger Label").Inline().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("The Biggest Label").Inline().SetContent(tss.UI.TextBox$1())]), tss.ICX.PaddingBottom(tss.txt, tss.ICX.PaddingTop(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Inline with auto-width")), tss.usX.px$1(16)), tss.usX.px$1(8)), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Lbl").Inline().AutoWidth().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Label").Inline().AutoWidth().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Bigger Label").Inline().AutoWidth().SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("The Biggest Label").Inline().AutoWidth().SetContent(tss.UI.TextBox$1())]), tss.ICX.PaddingBottom(tss.txt, tss.ICX.PaddingTop(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Inline with auto-width, aligned right")), tss.usX.px$1(16)), tss.usX.px$1(8)), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Lbl").Inline().AutoWidth(1, true).SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Label").Inline().AutoWidth(1, true).SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("Bigger Label").Inline().AutoWidth(1, true).SetContent(tss.UI.TextBox$1()), tss.UI.Label$1("The Biggest Label").Inline().AutoWidth(1, true).SetContent(tss.UI.TextBox$1())])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("LayerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Layer is a technical component that does not have specific Design guidance."), tss.UI.TextBlock("Layers are used to render content outside of a DOM tree, at the end of the document. This allows content to escape traditional boundaries caused by \"overflow: hidden\" css rules and keeps it on the top without using z-index rules. This is useful for example in ContextualMenu and Tooltip scenarios, where the content should always overlay everything else.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic layered content")), tss.LayerExtensions.Content(tss.Layer, layer, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("This is example layer content."), tss.UI.Button$1("Show second Layer").SetIcon$1(23).Primary().OnClick(function (s, e) {
                    layer2.IsVisible = true;
                }), tss.LayerExtensions.Content(tss.Layer, layer2, tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.TextBlock("This is the second example layer content."), tss.UI.Button$1("Hide second Layer").SetIcon$1(1056).Primary().OnClick(function (s, e) {
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
                tss.LayerExtensions.Content(tss.Modal, tss.ICX.Height(tss.Modal, tss.ICX.Width(tss.Modal, tss.UI.Var(tss.Modal, tss.UI.Modal("Lorem Ipsum"), modal).LightDismiss(), tss.usX.vw$1(60)), tss.usX.vh$1(60)).SetFooter(tss.ITFX.MediumPlus(tss.txt, tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("This is a footer note")))), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas lorem nulla, malesuada ut sagittis sit amet, vulputate in leo. Maecenas vulputate congue sapien eu tincidunt. Etiam eu sem turpis. Fusce tempor sagittis nunc, ut interdum ipsum vestibulum non. Proin dolor elit, aliquam eget tincidunt non, vestibulum ut turpis. In hac habitasse platea dictumst. In a odio eget enim porttitor maximus. Aliquam nulla nibh, ullamcorper aliquam placerat eu, viverra et dui. Phasellus ex lectus, maximus in mollis ac, luctus vel eros. Vivamus ultrices, turpis sed malesuada gravida, eros ipsum venenatis elit, et volutpat eros dui et ante. Quisque ultricies mi nec leo ultricies mollis. Vivamus egestas volutpat lacinia. Quisque pharetra eleifend efficitur. "), tss.UI.TextBlock("Mauris at nunc eget lectus lobortis facilisis et eget magna. Vestibulum venenatis augue sapien, rhoncus faucibus magna semper eget. Proin rutrum libero sagittis sapien aliquet auctor. Suspendisse tristique a magna at facilisis. Duis rhoncus feugiat magna in rutrum. Suspendisse semper, dolor et vestibulum lacinia, nunc felis malesuada ex, nec hendrerit justo ex et massa. Quisque quis mollis nulla. Nam commodo est ornare, rhoncus odio eu, pharetra tellus. Nunc sed velit mi."), tss.UI.TextBlock("Sed condimentum ultricies turpis convallis pharetra. Sed sagittis quam pharetra luctus porttitor. Cras vel consequat lectus. Sed nec fringilla urna, a aliquet libero. Aenean sed nisl purus. Vivamus vulputate felis et odio efficitur suscipit. Ut volutpat dictum lectus, ac rutrum massa accumsan at. Sed pharetra auctor finibus. In augue libero, commodo vitae nisi non, sagittis convallis ante. Phasellus malesuada eleifend mollis. Curabitur ultricies leo ac metus venenatis elementum."), tss.UI.TextBlock("Aenean egestas quam ut erat commodo blandit. Mauris ante nisl, pellentesque sed venenatis nec, aliquet sit amet enim. Praesent vitae diam non diam aliquet tristique non ut arcu. Pellentesque et ultrices eros. Fusce diam metus, mattis eu luctus nec, facilisis vel erat. Nam a lacus quis tellus gravida euismod. Nulla sed sem eget tortor cursus interdum. Sed vehicula tristique ultricies. Aenean libero purus, mollis quis massa quis, eleifend dictum massa. Fusce eu sapien sit amet odio lacinia placerat. Mauris varius risus sed aliquet cursus. Aenean lectus magna, tincidunt sit amet sodales a, volutpat ac leo. Morbi nisl sapien, tincidunt sit amet mauris quis, sollicitudin auctor est."), tss.UI.TextBlock("Nam id mi justo. Nam vehicula vulputate augue, ac pretium enim rutrum ultricies. Sed aliquet accumsan varius. Quisque ac auctor ligula. Fusce fringilla, odio et dignissim iaculis, est lacus ultrices risus, vitae condimentum enim urna eu nunc. In risus est, mattis non suscipit at, mattis ut ante. Maecenas consectetur urna vel erat maximus, non molestie massa consequat. Duis a feugiat nibh. Sed a hendrerit diam, a mattis est. In augue dolor, faucibus vel metus at, convallis rhoncus dui."), tss.UI.Label$1("Light Dismiss").Inline().AutoWidth().SetContent(tss.UI.Toggle().OnChange(function (s, e) {
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ModalSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Modals are temporary, modal UI overlay that generally provide contextual app information or require user confirmation/input, or can be used to advertise new app features. In some cases, Modals block interactions with the web page or application until being explicitly dismissed. They can be used for lightweight creation or edit tasks and simple management tasks, or for hosting heavier temporary content."), tss.UI.TextBlock("For usage requiring a quick choice from the user, Dialog may be a more appropriate control.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use Modals for actionable interactions, such as needing the user to provide information or change settings."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("When possible, try a non-blocking Modal before resorting to a blocking Modal."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Always have at least one focusable element inside a Modal.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t overuse Modals. In some cases they can be perceived as interrupting workflow, and too many can be a bad user experience.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Modal").OnClick(function (s, e) {
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

    H5.define("Tesserae.Tests.Samples.NumberPickerSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("NumberPickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The NumberPicker allows users to pick a number from a native browser widget."), tss.UI.Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/number", "Please see here for further information.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic NumberPicker")), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Standard").SetContent(tss.UI.NumberPicker()), tss.UI.Label$1("With default initial value of 2").SetContent(tss.UI.NumberPicker(2)), tss.UI.Label$1("With step increment of 10").SetContent(tss.UI.NumberPicker().SetStep(10)), tss.UI.Label$1(System.String.format("With max of 10", null)).SetContent(tss.UI.NumberPicker().SetMax(10)), tss.UI.Label$1(System.String.format("With min of 5", null)).SetContent(tss.UI.NumberPicker(8).SetMin(5)), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.NumberPicker().Disabled()), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.NumberPicker()), tss.UI.NumberPicker().Required(), tss.UI.Label$1("With error message").SetContent(tss.ICVX.IsInvalid(tss.NumberPicker, tss.ICVX.Error(tss.NumberPicker, tss.UI.NumberPicker(), "Error message"))), tss.UI.Label$1("With validation").SetContent(tss.vX.Validation(tss.NumberPicker, tss.UI.NumberPicker(), function (dateTimePicker) {
                    return dateTimePicker.Value % 2 === 0 ? null : "Please choose an even value";
                }))])]));
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
                var msg = { };
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("OverflowSetSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Breadcrumbs should be used as a navigational aid in your app or site. They indicate the current page\u2019s location within a hierarchy and help the user understand where they are in relation to the rest of that hierarchy. They also afford one-click access to higher levels of that hierarchy."), tss.UI.TextBlock("Breadcrumbs are typically placed, in horizontal form, under the masthead or navigation of an experience, above the primary content area.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Place Breadcrumbs at the top of a page, above a list of items, or above the main content of a page.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use Breadcrumbs as a primary way to navigate an app or site.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Label$1("Selected: ").SetContent(tss.UI.Var(tss.txt, tss.UI.TextBlock$1(), msg)), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("All Visible")), tss.ICX.PaddingBottom(tss.OverflowSet, tss.ICX.PaddingTop(tss.OverflowSet, tss.UI.OverflowSet(), tss.usX.px$1(16)), tss.usX.px$1(16)).Items([tss.UI.Button$1("Folder 1").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Button$1("Folder 2").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }).Disabled(), tss.UI.Button$1("Folder 3").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Button$1("Folder 4").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Button$1("Folder 5").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Button$1("Folder 6").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("All Visible, Small")), tss.ICX.PaddingBottom(tss.OverflowSet, tss.ICX.PaddingTop(tss.OverflowSet, tss.UI.OverflowSet().Small(), tss.usX.px$1(16)), tss.usX.px$1(16)).Items([tss.UI.Button$1("Folder 1").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Button$1("Folder 2").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }).Disabled(), tss.UI.Button$1("Folder 3").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Button$1("Folder 4").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Button$1("Folder 5").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Button$1("Folder 6").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 200px")), tss.ICX.MaxWidth(tss.OverflowSet, tss.ICX.PaddingBottom(tss.OverflowSet, tss.ICX.PaddingTop(tss.OverflowSet, tss.UI.OverflowSet(), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(200)).Items([tss.UI.Button$1("Folder 1").Link().SetIcon$1(20).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Button$1("Folder 2").Link().SetIcon$1(1033).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Button$1("Folder 3").Link().SetIcon$1(238).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Button$1("Folder 4").Link().SetIcon$1(3330).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Button$1("Folder 5").Link().SetIcon$1(2577).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Button$1("Folder 6").Link().SetIcon$1(2651).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 200px, Small")), tss.ICX.MaxWidth(tss.OverflowSet, tss.ICX.PaddingBottom(tss.OverflowSet, tss.ICX.PaddingTop(tss.OverflowSet, tss.UI.OverflowSet(), tss.usX.px$1(16)), tss.usX.px$1(16)).Small(), tss.usX.px$1(200)).Items([tss.UI.Button$1("Folder 1").Link().SetIcon$1(20).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Button$1("Folder 2").Link().SetIcon$1(1033).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Button$1("Folder 3").Link().SetIcon$1(238).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Button$1("Folder 4").Link().SetIcon$1(3330).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Button$1("Folder 5").Link().SetIcon$1(2577).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Button$1("Folder 6").Link().SetIcon$1(2651).OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 300px")), tss.ICX.MaxWidth(tss.OverflowSet, tss.ICX.PaddingBottom(tss.OverflowSet, tss.ICX.PaddingTop(tss.OverflowSet, tss.UI.OverflowSet(), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(300)).Items([tss.UI.Button$1("Folder 1").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Button$1("Folder 2").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Button$1("Folder 3").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Button$1("Folder 4").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Button$1("Folder 5").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Button$1("Folder 6").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 300px, from second, custom chevron")), tss.ICX.MaxWidth(tss.OverflowSet, tss.ICX.PaddingBottom(tss.OverflowSet, tss.ICX.PaddingTop(tss.OverflowSet, tss.UI.OverflowSet(), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(300)).SetOverflowIndex(1).Items([tss.UI.Button$1("Folder 1").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.Button$1("Folder 2").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.Button$1("Folder 3").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.Button$1("Folder 4").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.Button$1("Folder 5").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.Button$1("Folder 6").Link().OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })])]));
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
                    panel.Side = tss.Panel.PanelSide.Far;
                }), tss.UI.Choice("Near").OnSelected(function (x) {
                    panel.Side = tss.Panel.PanelSide.Near;
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PanelSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Panels are modal UI overlays that provide contextual app information. They often request some kind of creation or management action from the user. Panels are paired with the Overlay component, also known as a Light Dismiss. The Overlay blocks interactions with the app view until dismissed either through clicking or tapping on the Overlay or by selecting a close or completion action within the Panel."), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Examples of experiences that use Panels"), tss.UI.TextBlock("Member or group list creation or management"), tss.UI.TextBlock("Document list creation or management"), tss.UI.TextBlock("Permissions creation or management"), tss.UI.TextBlock("Settings creation or management"), tss.UI.TextBlock("Multi-field forms")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use for self-contained experiences where the user does not need to interact with the app view to complete the task."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use for complex creation, edit or management experiences."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Consider how the panel and its contained contents will scale across Fabric\u2019s responsive web breakpoints.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use for experiences where the user needs to interact with the app view.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open panel").OnClick(function (s, e) {
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PickerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Pickers are used to pick recipients.")])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Picker with text suggestions and tag-like selections")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.Picker(Tesserae.Tests.Samples.PickerSampleItem), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItem, 2147483647, false, 0, true, "Suggested Tags").Items(this.GetPickerItems()), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Picker with single selection")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.Picker(Tesserae.Tests.Samples.PickerSampleItem), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItem, 1, false, 0, true, "Suggested Tags").Items(this.GetPickerItems()), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Picker with icon and text suggestions and component based selections")), tss.usX.px$1(16)), tss.UI.Picker(Tesserae.Tests.Samples.PickerSampleItemWithComponents, 2147483647, false, 0, false, "Suggested Items").Items(this.GetComponentPickerItems())]));
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
                return System.Array.init([new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Bob", 380), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("BOB", 356), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Donuts by J Dilla", 667), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Donuts", 627), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Coffee", 946), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Chicken Coop", 1713), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Cherry Pie", 714), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Chess", 751), new Tesserae.Tests.Samples.PickerSampleItemWithComponents("Cooper", 1938)], Tesserae.Tests.Samples.PickerSampleItemWithComponents);
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
                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("PivotSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TODO"), tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock("Examples of experiences that use Panels"))])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("TODO")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("TODO")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Cached vs. Not Cached Tabs"), tss.PivotX.Pivot(tss.PivotX.Pivot(tss.UI.Pivot(), "tab1", function () {
                    return tss.UI.Button$1().SetText("Cached").NoBorder().NoBackground().Link();
                }, function () {
                    return tss.ITFX.Regular(tss.txt, tss.UI.TextBlock(System.DateTimeOffset.UtcNow.toString()));
                }, true), "tab2", function () {
                    return tss.UI.Button$1().SetText("Not Cached").SetIcon$1(2769).NoBorder().NoBackground().Link();
                }, function () {
                    return tss.ITFX.Regular(tss.txt, tss.UI.TextBlock(System.DateTimeOffset.UtcNow.toString()));
                }, false), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Cached vs. Not Cached Tabs"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Scroll with limited height"), tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.ICX.MaxHeight(tss.Pivot, tss.UI.Pivot(), tss.usX.px$1(500)), "tab1", function () {
                    return tss.UI.Button$1().SetText("5 Items").NoBorder().NoBackground().Link();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), tss.usX.px$1(16));
                }), true), "tab2", function () {
                    return tss.UI.Button$1().SetText("10 Items").NoBorder().NoBackground().Link();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(20)), tss.usX.px$1(16));
                }), true), "tab3", function () {
                    return tss.UI.Button$1().SetText("50 Items").NoBorder().NoBackground().Link();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(50)), tss.usX.px$1(16));
                }), true), "tab4", function () {
                    return tss.UI.Button$1().SetText("100 Items").NoBorder().NoBackground().Link();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(100)), tss.usX.px$1(16));
                }), true), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Too many tabs (WIP)"), tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.PivotX.Pivot(tss.ICX.MaxWidth(tss.Pivot, tss.ICX.MaxHeight(tss.Pivot, tss.UI.Pivot(), tss.usX.px$1(500)), tss.usX.px$1(300)), "tab1", function () {
                    return tss.UI.Button$1().SetText("5 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), tss.usX.px$1(16));
                }), true), "tab2", function () {
                    return tss.UI.Button$1().SetText("10 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(20)), tss.usX.px$1(16));
                }), true), "tab3", function () {
                    return tss.UI.Button$1().SetText("50 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(50)), tss.usX.px$1(16));
                }), true), "tab4", function () {
                    return tss.UI.Button$1().SetText("100 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(100)), tss.usX.px$1(16));
                }), true), "tab5", function () {
                    return tss.UI.Button$1().SetText("5 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(5)), tss.usX.px$1(16));
                }), true), "tab6", function () {
                    return tss.UI.Button$1().SetText("10 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(20)), tss.usX.px$1(16));
                }), true), "tab7", function () {
                    return tss.UI.Button$1().SetText("50 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(50)), tss.usX.px$1(16));
                }), true), "tab8", function () {
                    return tss.UI.Button$1().SetText("100 Items").NoBorder().NoBackground().Link().Ellipsis();
                }, H5.fn.bind(this, function () {
                    return tss.ICX.PaddingBottom(tss.ItemsList, tss.UI.ItemsList(this.GetSomeItems(100)), tss.usX.px$1(16));
                }), true)]));
            }
        },
        methods: {
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

    H5.define("Tesserae.Tests.Samples.ProgressIndicatorSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ProgressIndicatorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("ProgressIndicators are used to show the completion status of an operation lasting more than 2 seconds. If the state of progress cannot be determined, use a Spinner instead. ProgressIndicators can appear in a new panel, a flyout, under the UI initiating the operation, or even replacing the initiating UI, as long as the UI can return if the operation is canceled or is stopped.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()).Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use a ProgressIndicator when the total units to completion is known"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Display operation description"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Show text above and/or below the bar"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Combine steps of a single operation into one bar")]), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Use a ProgressIndicator when the total units to completion is indeterminate."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Show text to the right or left of the bar"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Cause progress to \u201crewind\u201d to show new steps")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("States")), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Empty").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(0), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("30%").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(30), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("60%").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(60), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Full").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Progress$1(100), tss.usX.px$1(400)))), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Indeterminate").SetContent(tss.ICX.Width(tss.ProgressIndicator, tss.UI.ProgressIndicator().Indeterminated(), tss.usX.px$1(400))))]));
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
                var PlayModal = null;
                var ProgressFrame = null;
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
                    var $s = 0,
                        $t1, 
                        $jff, 
                        $tcs = new H5.TCS(), 
                        $rv, 
                        $ae, 
                        $ab = H5.fn.bind(this, function () {
                            try {
                                for (;;) {
                                    $s = H5.STEP([0,1], $s);
                                    switch ($s) {
                                        case 0: {
                                            modal = tss.UI.ProgressModal().Title("Lorem Ipsum");
                                            cts = new System.Threading.CancellationTokenSource();
                                            modal.WithCancel(function (b) {
                                                b.Disabled();
                                                cts.cancel();
                                            });
                                            progress = 0;
                                            modal.Message("Preparing to process...").ProgressSpin().Show();
                                            $t1 = System.Threading.Tasks.Task.delay(1500);
                                            $s = 1;
                                            if ($t1.isC()) {
                                                continue;
                                            }
                                            $t1.c($ab);
                                            return;
                                        }
                                        case 1: {
                                            $t1.gAR();
                                            window.setTimeout(ProgressFrame, 16);
                                            $tcs.sR(null);
                                            return;
                                        }
                                        default: {
                                            $tcs.sR(null);
                                            return;
                                        }
                                    }
                                }
                            } catch($ae1) {
                                $ae = System.Exception.create($ae1);
                                $tcs.sE($ae);
                            }
                        }, arguments);

                    $ab();
                    return $tcs.task;
                };

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ProgressModalSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TODO")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("TODO")]), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("TODO")])])])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Modal").OnClick(function (s, e) {
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

    H5.define("Tesserae.Tests.Samples.SearchableGroupedListSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var GroupedItemHeaderGenerator = null;
                GroupedItemHeaderGenerator = function (s) {
                    return tss.UI.HorizontalSeparator$1(tss.ITFX.SemiBold(tss.txt, tss.txtX.Primary(tss.txt, tss.UI.TextBlock(s)))).Left();
                };
                this._content = tss.ICX.PaddingBottom(tss.SectionStack, tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SearchableGroupedListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("This list provides a base component for implementing search over a known number of items.It is agnostic of the tile component used, and selection management. These concerns can be layered separately."), tss.usX.px$1(16)), tss.UI.TextBlock("You need to implement ISearchableGroupedItem interface on the items, and specially the IsMatch method to enable searching on them")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingTop(tss.txt, tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Searchable Grouped List with No Results Message")), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.ICX.PaddingBottom(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(10), GroupedItemHeaderGenerator), tss.usX.px$1(32)), tss.usX.px$1(500)).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HeightStretch(tss.BackgroundArea, tss.ICX.WidthStretch(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No Results"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.ICX.PaddingTop(tss.txt, tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Searchable Grouped List with extra commands")), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.ICX.PaddingBottom(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(10), GroupedItemHeaderGenerator), tss.usX.px$1(32)), tss.usX.px$1(500)).AfterSearchBox([tss.UI.Button$1("Sample Button After").Primary()]).BeforeSearchBox([tss.UI.Button$1("Sample Button Before").Link()]).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HeightStretch(tss.BackgroundArea, tss.ICX.WidthStretch(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No Results"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.ICX.PaddingTop(tss.txt, tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Searchable Grouped List with Columns")), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.ICX.Height(tss.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem), tss.UI.SearchableGroupedList(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem, this.GetItems(40), GroupedItemHeaderGenerator, [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)]), tss.usX.px$1(500))])), tss.usX.px$1(32));


            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (number, index) {
                    var group = "";

                    if (index % 2 === 0) {
                        group = "Group A";
                    } else if (index % 3 === 0) {
                        group = "Group B";
                    } else {
                        group = "Group C";
                    }

                    return new Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem(System.String.format("Lorem Ipsum {0}", [H5.box(number, System.Int32)]), group);
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
                this._content = tss.ICX.PaddingBottom(tss.SectionStack, tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SearchableListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("This list provides a base component for implementing search over a known number of items.It is agnostic of the tile component used, and selection management. These concerns can be layered separately."), tss.usX.px$1(16)), tss.UI.TextBlock("You need to implement ISearchableItem interface on the items, and specially the IsMatch method to enable searching on them")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingTop(tss.txt, tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Searchable List with No Results Message")), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.ICX.PaddingBottom(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(10)), tss.usX.px$1(32)), tss.usX.px$1(500)).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HeightStretch(tss.BackgroundArea, tss.ICX.WidthStretch(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No Results"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.ICX.PaddingTop(tss.txt, tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Searchable List with extra commands")), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.ICX.PaddingBottom(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(10)), tss.usX.px$1(32)), tss.usX.px$1(500)).AfterSearchBox([tss.UI.Button$1("Sample Button After").Primary()]).BeforeSearchBox([tss.UI.Button$1("Sample Button Before").Link()]).WithNoResultsMessage(function () {
                    return tss.ICX.MinHeight(tss.BackgroundArea, tss.ICX.HeightStretch(tss.BackgroundArea, tss.ICX.WidthStretch(tss.BackgroundArea, tss.UI.BackgroundArea(tss.UI.Card(tss.ICX.Padding(tss.txt, tss.UI.TextBlock("No Results"), tss.usX.px$1(16)))))), tss.usX.px$1(100));
                }), tss.ICX.PaddingTop(tss.txt, tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Searchable List with Columns")), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.ICX.Height(tss.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem), tss.UI.SearchableList(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem, this.GetItems(40), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)]), tss.usX.px$1(500))])), tss.usX.px$1(32));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (number) {
                    return new Tesserae.Tests.Samples.SearchableListSample.SearchableListItem(System.String.format("Lorem Ipsum {0}", [H5.box(number, System.Int32)]));
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
                var searchAsYouType = tss.UI.TextBlock("start typing");

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SearchBoxSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("SearchBoxes provide an input field for searching through content, allowing users to locate specific items within the website or app.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use placeholder text in the SearchBox to describe what users can search for."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Example: 'Search'; 'Search files'; 'Search site'"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Once the user has clicked into the SearchBox but hasn\u2019t entered input yet, use 'hint text' to communicate search scope."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Examples: 'Try searching for a PDFs'; 'Search contacts list'; 'Type to find <content type> '"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use the Underlined SearchBox for CommandBars.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't leave the SearchBox blank because it's too ambiguous."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't have lengthy and unclear hint text. It should be used to clasify and set expectations."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't provide inaccurate matches or bad predictions, as it will make search seem unreliable and will result in user frustration."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t provide too much information or metadata in the suggestions list; it\u2019s intended to be lightweight."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't build a custom search control based on the default text box or any other control."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use SearchBox if you cannot reliably provide accurate results.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic TextBox")), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Default").SetContent(tss.UI.SearchBox("Search").OnSearch(function (s, e) {
                    alert(System.String.format("Searched for {0}", [e]));
                })), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.SearchBox("Search").Disabled()), tss.UI.Label$1("Underline").SetContent(tss.UI.SearchBox("Search").Underlined().OnSearch(function (s, e) {
                    alert(System.String.format("Searched for {0}", [e]));
                })), tss.UI.Label$1("Search as you type").SetContent(tss.UI.SearchBox("Search").Underlined().SearchAsYouType().OnSearch(function (s, e) {
                    searchAsYouType.Text = System.String.format("Searched for {0}", [e]);
                })), searchAsYouType, tss.txtX.Required(tss.Label, tss.UI.Label$1("Custom Icon")).SetContent(tss.UI.SearchBox("Filter").SetIcon(1485).OnSearch(function (s, e) {
                    alert(System.String.format("Filter for {0}", [e]));
                })), tss.UI.Label$1("No Icon").SetContent(tss.UI.SearchBox("Search").NoIcon().OnSearch(function (s, e) {
                    alert(System.String.format("Searched for {0}", [e]));
                })), tss.txtX.Required(tss.Label, tss.UI.Label$1("Fixed Width")).SetContent(tss.ICX.Width(tss.SearchBox, tss.UI.SearchBox("Small Search"), tss.usX.px$1(200)).OnSearch(function (s, e) {
                    alert(System.String.format("Searched for {0}", [e]));
                }))])]));
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

                this._content = tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SectionStackSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Session Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Label$1("Number of items:").SetContent(tss.UI.Slider(5, 0, 10, 1).OnInput(H5.fn.bind(this, function (s, e) {
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

    H5.define("Tesserae.Tests.Samples.SliderSample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                this.$initialize();
                var value = new (tss.SettableObservableT(System.Int32))(0);
                var s1 = tss.UI.Slider(0, 0, 100, 1).OnInput(function (s, e) {
                    value.Update(function (currVal) {
                        value.Value$1 = s.Value;
                    });
                });
                var s2 = tss.UI.Slider(0, 0, 100, 10).OnInput(function (s, e) {
                    value.Update(function (currVal) {
                        value.Value$1 = s.Value;
                    });
                });
                value.Observe(function (changedValue) {
                    s1.Value = changedValue;
                });

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SliderSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TODO")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("TODO")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("TODO")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.Label, tss.UI.Label$1("Value")).SetContent(tss.UI.DeferSync$3(System.Int32, value, function (currentValue) {
                    return tss.UI.TextBlock(H5.toString(currentValue));
                })), tss.ITFX.Medium(tss.Label, tss.UI.Label$1("Default Slider (val: 0, min: 0, max: 100, step: 1)")).SetContent(s1), tss.ITFX.Medium(tss.Label, tss.UI.Label$1("Default Slider (val: 0, min: 0, max: 100, step: 10)")).SetContent(s2)]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SpinnerSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Spinner is an outline of a circle which animates around itself indicating to the user that things are processing. A Spinner is shown when it's unsure how long a task will take making it the indeterminate version of a ProgressIndicator. They can be various sizes, located inline with content or centered. They generally appear after an action is being processed or committed. They are subtle and generally do not take up much space, but are transitions from the completed task.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()).Horizontal(), [tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use a Spinner when a task is not immediate."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use one Spinner at a time."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Descriptive verbs are appropriate under a Spinner to help the user understand what's happening. Ie: Saving, processing, updating."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use a Spinner when confirming a change has been made or a task is being processed.")]), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t use a Spinner when performing immediate tasks."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't show multiple Spinners at the same time."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't include more than a few words when paired with a Spinner.")])])])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner sizes")), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Extra small spinner").SetContent(tss.UI.Spinner().XSmall())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Small spinner").SetContent(tss.UI.Spinner().Small())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Medium spinner").SetContent(tss.UI.Spinner().Medium())), tss.ICX.AlignCenter(tss.Label, tss.UI.Label$1("Large spinner").SetContent(tss.UI.Spinner().Large()))])), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.px$1(400)), [tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Spinner label positioning")), tss.UI.Label$1("Spinner with label positioned below").SetContent(tss.UI.Spinner("I am definitely loading...").Below()), tss.UI.Label$1("Spinner with label positioned above").SetContent(tss.UI.Spinner("Seriously, still loading...").Above()), tss.UI.Label$1("Spinner with label positioned to right").SetContent(tss.UI.Spinner("Wait, wait...").Right()), tss.UI.Label$1("Spinner with label positioned to left").SetContent(tss.UI.Spinner("Nope, still loading...").Left())]));
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
                var splitView = tss.UI.SplitView();
                var horzSplitView = tss.UI.HorizontalSplitView().BottomIsSmaller(tss.usX.px$1(50));

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("SplitViewSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TODO")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("TODO")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("TODO")])])])), tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.HStack()), [tss.UI.Button$1("Resizable").OnClick$1(function () {
                    splitView.Resizable();
                    horzSplitView.Resizable();
                }), tss.UI.Button$1("Non-resizable").OnClick$1(function () {
                    splitView.NotResizable();
                    horzSplitView.NotResizable();
                })]), tss.ICX.Grow(tss.SplitView, tss.ICX.H(tss.SplitView, tss.ICX.WS(tss.SplitView, tss.ICX.P(tss.SplitView, splitView.Left(tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), "green")).Right(tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), "blue")).Resizable(), 10)), 10)), tss.ICX.Grow(tss.HorizontalSplitView, tss.ICX.H(tss.HorizontalSplitView, tss.ICX.WS(tss.HorizontalSplitView, tss.ICX.P(tss.HorizontalSplitView, horzSplitView.Top(tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), "green")).Bottom(tss.ISX.Background(tss.S, tss.ICX.S(tss.S, tss.UI.Stack()), "blue")).Resizable(), 10)), 10))]), true, false, "");
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
                var mainButton = tss.ICX.MinWidth(tss.Button, tss.ITFX.TextLeft(tss.Button, tss.UI.Button$1("Some Text")), tss.usX.px$1(200)).Ellipsis().IconOnHover();
                tss.ICX.Tooltip$1(tss.Button, mainButton, "Tooltip for the main Button").SetIcon$1(83, tss.UI.Theme.Primary.Background);

                var otherButton = tss.ICX.Fade$2(tss.Button, tss.ICX.Tooltip$1(tss.Button, tss.UI.Button$1(), "Tooltip for the other Button").SetIcon$1(3399, tss.UI.Theme.Danger.Background, "tss-fontsize-small", "fi-rr-", false));
                var hoverStack = tss.ICTX.Children$6(tss.S, tss.ICX.MaxWidth(tss.S, tss.UI.HStack(), tss.usX.px$1(500)), [mainButton, otherButton]);
                var sortableStack = tss.ICX.MaxWidth(Tesserae.SortableStack, tss.ICX.PB(Tesserae.SortableStack, tss.ICX.WS(Tesserae.SortableStack, new Tesserae.SortableStack(tss.S.Orientation.Horizontal)).AlignItemsCenter(), 8), tss.usX.px$1(500));

                sortableStack.Add("1", tss.UI.Button$1().SetIcon$1(3));
                sortableStack.Add("2", tss.UI.Button$1().SetIcon$1(4));
                sortableStack.Add("3", tss.UI.Button$1().SetIcon$1(5));
                sortableStack.Add("4", tss.UI.Button$1().SetIcon$1(7));
                sortableStack.Add("5", tss.UI.Button$1().SetIcon$1(9));

                var sortingTimeout = 0.0;

                sortableStack.OnSortingChanged(function (stackOrder) {
                    window.clearTimeout(sortingTimeout);

                    sortingTimeout = window.setTimeout(function (_) {
                        localStorage.setItem(Tesserae.Tests.Samples.StackSample.sampleSortableStackLocalStorageKey, JSON.stringify(stackOrder));
                        console.log.apply(console, ["saved sorting for sortable stack sample"].concat(stackOrder));
                    }, 1000);
                });

                var sortableStackSampleOrderJson = localStorage.getItem(Tesserae.Tests.Samples.StackSample.sampleSortableStackLocalStorageKey);

                if (H5.is(sortableStackSampleOrderJson, System.Object)) {
                    var sortableStackSampleOrder = JSON.parse(sortableStackSampleOrderJson);
                    console.log("loaded sorting", H5.unbox(sortableStackSampleOrder));
                    sortableStack.LoadSorting(H5.unbox(sortableStackSampleOrder));
                }

                var stack = tss.UI.Stack();
                var countSlider = tss.UI.Slider(5, 0, 10, 1);

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("StackSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.UI.Label$1("Number of items:").SetContent(countSlider.OnInput(H5.fn.bind(this, function (s, e) {
                    this.SetChildren(stack, s.Value);
                }))), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.ChoiceGroup("Orientation:").Horizontal().Choices([tss.UI.Choice("Vertical").Selected(), tss.UI.Choice("Horizontal"), tss.UI.Choice("Vertical Reverse"), tss.UI.Choice("Horizontal Reverse")]).OnChange(function (s, e) {
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
                })])])]), tss.ICX.HeightAuto(tss.S, stack)])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Sortable"), tss.UI.Label$1("Stack with sortable elements").SetContent(sortableStack)])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Advanced"), tss.UI.Label$1("Stack with hover events").SetContent(tss.ICTX.Children$6(tss.S, hoverStack.OnMouseOver(function (s, e) {
                    tss.ICX.Show(tss.Button, otherButton);
                }).OnMouseOut(function (s, e) {
                    tss.ICX.Fade$2(tss.Button, otherButton);
                }), [tss.ICX.WS(tss.Button, mainButton), otherButton]))]));
                this.SetChildren(stack, 5);
            }
        },
        methods: {
            SetChildren: function (stack, count) {
                stack.Clear();

                for (var i = 0; i < count; i = (i + 1) | 0) {
                    stack.Add(tss.UI.Button$1(H5.toString(i)));
                }
            },
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TextBlockSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Text is a component for displaying text. You can use Text to standardize text across your web app.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("TextBox Ramp Example")), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ITFX.SemiBold(tss.txt, tss.ICX.Width(tss.txt, tss.UI.TextBlock("Variant"), tss.usX.px$1(200))), tss.ITFX.SemiBold(tss.txt, tss.UI.TextBlock("Example"))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("tiny"), tss.usX.px$1(200)), tss.ITFX.Tiny(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("xSmall"), tss.usX.px$1(200)), tss.ITFX.XSmall(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("small"), tss.usX.px$1(200)), tss.ITFX.Small(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("smallPlus"), tss.usX.px$1(200)), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("medium"), tss.usX.px$1(200)), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("mediumPlus"), tss.usX.px$1(200)), tss.ITFX.MediumPlus(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("large"), tss.usX.px$1(200)), tss.ITFX.Large(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("xLarge"), tss.usX.px$1(200)), tss.ITFX.XLarge(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("xxLarge"), tss.usX.px$1(200)), tss.ITFX.XXLarge(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICX.Width(tss.txt, tss.UI.TextBlock("mega"), tss.usX.px$1(200)), tss.ITFX.Mega(tss.txt, tss.UI.TextBlock("The quick brown fox jumped over the lazy dog."))]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("TextBox Wrap Example")), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("Wrap (Default)")), tss.ICX.Width(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."), tss.usX.percent$1(50)), tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock("No Wrap")), tss.ICX.WS(tss.txt, tss.ITFX.SmallPlus(tss.txt, tss.UI.TextBlock$1("This is a very long text that can wrap but from here on it will never wrap:", false, false, "tss-fontsize-small", "tss-fontweight-regular", "this will not wrap"))), tss.ICX.Width(tss.txt, tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")), tss.usX.percent$1(50))]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TextBoxSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The TextBox component enables a user to type text into an app. The text displays on the screen in a simple, uniform format.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use the TextBox to accept data input on a form or page."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Label the TextBox with a helpful name."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Provide concise helper text that specifies what content is expected to be entered."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("When part of a form, provide clear designations for which TextBox are required vs. optional."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Provide all appropriate methods for submitting provided data (e.g. dedicated \u2018Submit\u2019 button)."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Provide all appropriate methods of clearing provided data (\u2018X\u2019 or something similar)."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Allow for selection, copy and paste of field data."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Ensure that the TextBox is functional through use of mouse/keyboard or touch when available.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use a TextBox to render basic copy as part of a body element of a page."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't provide an unlabeled TextBox and expect that users will know what to do with it."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't place a TextBox inline with body copy."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't be overly verbose with helper text."), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't occlude the entry or allow entry when the active content is not visible.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic TextBox")), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [tss.UI.Label$1("Standard").SetContent(tss.UI.TextBox$1()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled")).SetContent(tss.UI.TextBox$1("I am disabled").Disabled()), tss.UI.Label$1("Read-only").SetContent(tss.UI.TextBox$1("I am read-only").ReadOnly()), tss.UI.Label$1("Password").SetContent(tss.UI.TextBox$1("I am a password box").Password()), tss.txtX.Required(tss.Label, tss.UI.Label$1("Required")).SetContent(tss.UI.TextBox$1("")), tss.UI.TextBox$1("").Required(), tss.UI.Label$1("With error message").SetContent(tss.ICVX.IsInvalid(tss.TextBox, tss.ICVX.Error(tss.TextBox, tss.UI.TextBox$1(), "Error message"))), tss.UI.Label$1("With placeholder").SetContent(tss.UI.TextBox$1().SetPlaceholder("Please enter text here")), tss.UI.Label$1("With validation").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1(), function (tb) {
                    return tb.Text.length === 0 ? "Empty" : null;
                })), tss.UI.Label$1("With validation on type").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1(), tss.Validation.NonZeroPositiveInteger)), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled with placeholder")).SetContent(tss.UI.TextBox$1().SetPlaceholder("I am disabled").Disabled())])]));
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
                var msg = { };
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TextBreadcrumbsSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("TextBreadcrumbs should be used as a navigational aid in your app or site. They indicate the current page\u2019s location within a hierarchy and help the user understand where they are in relation to the rest of that hierarchy. They also afford one-click access to higher levels of that hierarchy."), tss.UI.TextBlock("TextBreadcrumbs are typically placed, in horizontal form, under the masthead or navigation of an experience, above the primary content area.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Place TextBreadcrumbs at the top of a page, above a list of items, or above the main content of a page.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don't use TextBreadcrumbs as a primary way to navigate an app or site.")])])])), tss.ICTX.Children$6(tss.S, tss.ICX.WS(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Label$1("Selected: ").SetContent(tss.UI.Var(tss.txt, tss.UI.TextBlock$1(), msg)), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("All Visible")), tss.ICX.PaddingBottom(tss.TextBreadcrumbs, tss.ICX.PaddingTop(tss.TextBreadcrumbs, tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs(), tss.usX.percent$1(100)), tss.usX.px$1(16)), tss.usX.px$1(16)).Items([tss.UI.TextBreadcrumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.TextBreadcrumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.TextBreadcrumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.TextBreadcrumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("All Visible, Small")), tss.ICX.PaddingBottom(tss.TextBreadcrumbs, tss.ICX.PaddingTop(tss.TextBreadcrumbs, tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs(), tss.usX.percent$1(100)), tss.usX.px$1(16)), tss.usX.px$1(16)).Items([tss.UI.TextBreadcrumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.TextBreadcrumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.TextBreadcrumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.TextBreadcrumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 200px")), tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.ICX.PaddingBottom(tss.TextBreadcrumbs, tss.ICX.PaddingTop(tss.TextBreadcrumbs, tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs(), tss.usX.percent$1(100)), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(200)).Items([tss.UI.TextBreadcrumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.TextBreadcrumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.TextBreadcrumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.TextBreadcrumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 200px, Small")), tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.ICX.PaddingBottom(tss.TextBreadcrumbs, tss.ICX.PaddingTop(tss.TextBreadcrumbs, tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs(), tss.usX.percent$1(100)), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(200)).Items([tss.UI.TextBreadcrumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.TextBreadcrumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.TextBreadcrumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.TextBreadcrumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 300px")), tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.ICX.PaddingBottom(tss.TextBreadcrumbs, tss.ICX.PaddingTop(tss.TextBreadcrumbs, tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs(), tss.usX.percent$1(100)), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(300)).Items([tss.UI.TextBreadcrumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.TextBreadcrumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.TextBreadcrumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.TextBreadcrumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })]), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Collapse 300px, from second, custom chevron")), tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.ICX.PaddingBottom(tss.TextBreadcrumbs, tss.ICX.PaddingTop(tss.TextBreadcrumbs, tss.ICX.MaxWidth(tss.TextBreadcrumbs, tss.UI.TextBreadcrumbs(), tss.usX.percent$1(100)), tss.usX.px$1(16)), tss.usX.px$1(16)), tss.usX.px$1(300)).Items([tss.UI.TextBreadcrumb("Folder 1").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 1");
                }), tss.UI.TextBreadcrumb("Folder 2").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 2");
                }), tss.UI.TextBreadcrumb("Folder 3").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 3");
                }), tss.UI.TextBreadcrumb("Folder 4").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 4");
                }), tss.UI.TextBreadcrumb("Folder 5").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 5");
                }), tss.UI.TextBreadcrumb("Folder 6").OnClick(function (s, e) {
                    tss.txtX.Text(tss.txt, msg.v, "Folder 6");
                })])]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ThemeColorsSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [tss.ICX.Height(tss.DetailsList(Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem), tss.UI.DetailsList(Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem, [tss.UI.DetailsListColumn("ThemeName", tss.usX.px$1(120), false, false, void 0, void 0), tss.UI.DetailsListColumn("Background", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("Foreground", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("Border", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("BackgroundActive", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("BackgroundHover", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("ForegroundActive", tss.usX.px$1(160), false, false, void 0, void 0), tss.UI.DetailsListColumn("ForegroundHover", tss.usX.px$1(160), false, false, void 0, void 0)]).Compact(), tss.usX.px$1(500)).WithListItems(System.Array.init([new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Default"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Primary"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Secondary"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Success"), new Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem("Danger")], Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem)).SortedBy("Name"), tss.UI.Label$1("Primary Light").Inline().SetContent(cpPrimaryLight), tss.UI.Label$1("Primary Dark").Inline().SetContent(cpPrimaryDark), tss.UI.Label$1("Background Light").Inline().SetContent(cpBackgroundLight), tss.UI.Label$1("Background Dark").Inline().SetContent(cpBackgroundDark)]));
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
                var obsList = new (tss.ObservableList(tss.IC)).ctor();

                var vs = tss.UI.VisibilitySensor(H5.fn.bind(this, function (v) {
                    obsList.remove(v);
                    obsList.AddRange(this.GetSomeItems(20));
                    v.Reset();
                    obsList.add(v);
                }));

                obsList.AddRange(this.GetSomeItems(10));
                obsList.add(vs);

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.WidthStretch(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TimelineSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("Timeline provides a base component for rendering vertical timelines. It is agnostic of the tile component used, and selection management. These concerns can be layered separately."), tss.usX.px$1(16))])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Timeline")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.Timeline, tss.ICX.Height(tss.Timeline, tss.ICX.PaddingBottom(tss.Timeline, tss.ICTX.Children$6(tss.Timeline, tss.UI.Timeline(), this.GetSomeItems(10)), tss.usX.px$1(16)), tss.usX.px$1(500)), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Timeline with Max Width")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.Timeline, tss.ICX.Height(tss.Timeline, tss.ICX.PaddingBottom(tss.Timeline, tss.ICTX.Children$6(tss.Timeline, tss.UI.Timeline().TimelineWidth(tss.usX.px$1(600)), this.GetSomeItems(10)), tss.usX.px$1(16)), tss.usX.px$1(500)), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Timeline Same Side")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.Timeline, tss.ICX.Height(tss.Timeline, tss.ICX.PaddingBottom(tss.Timeline, tss.ICTX.Children$6(tss.Timeline, tss.UI.Timeline().SameSide(), this.GetSomeItems(10)), tss.usX.px$1(16)), tss.usX.px$1(500)), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Timeline Same Side with Max Width")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.Timeline, tss.ICX.Height(tss.Timeline, tss.ICX.PaddingBottom(tss.Timeline, tss.ICTX.Children$6(tss.Timeline, tss.UI.Timeline().TimelineWidth(tss.usX.px$1(600)).SameSide(), this.GetSomeItems(10)), tss.usX.px$1(16)), tss.usX.px$1(500)), tss.usX.px$1(32))]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetSomeItems: function (count) {
                return System.Linq.Enumerable.range(1, count).select(function (number) {
                    return tss.txtX.NonSelectable(tss.txt, tss.UI.TextBlock(System.String.format("Lorem Ipsum {0}", [H5.box(number, System.Int32)])));
                }).ToArray(tss.txt);
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
                var _discard1 = { };

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TippySample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Use .Tooltip(...) to add tooltips to your components, or Tippy.ShowFor(...) to show them on demand.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICTX.Children$6(tss.S, tss.UI.VStack(), [tss.ICX.Tooltip$1(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Hover me"), 200), "This is a simple text tooltip"), tss.ICX.Tooltip$1(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Animated Tooltip"), 200), "This is a simple text tooltip with animations", "shift-away-subtle"), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Interactive Tooltip"), 200), tss.UI.Button$1("Click me").OnClick$1(function () {
                    tss.UI.Toast().Success("You clicked!");
                }), true, "none", "top", 250, 0, true, false, 350, true, false, void 0, void 0), tss.ICX.Tooltip(tss.Button, tss.ICX.W(tss.Button, tss.UI.Button$1("Nested Tooltips"), 200), tss.UI.Button$1("Click me").OnClick(function (b1, _) {
                    tss.tippy.ShowFor$1(b1, tss.UI.Button$1("Click me").OnClick$1(function () {
                        tss.UI.Toast().Success("You clicked!");
                    }), _discard1);
                }), true, "none", "top", 250, 0, true, false, 350, true, false, void 0, void 0)])]));
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ToastSample")), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Toasts are used for short-lived notifications to users.")])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.UI.SplitView().SplitInMiddle().Left(tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Write short and recognizable messages"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Keep toasts long enough to be read, but not long enough to bother")])).Right(tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Overload users with toasts.")]))])), tss.ICTX.Children$6(tss.S, tss.ICX.WidthStretch(tss.S, tss.UI.Stack()), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top-right (default)"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top left"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().TopLeft().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().TopLeft().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().TopLeft().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().TopLeft().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom right"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().BottomRight().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().BottomRight().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().BottomRight().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().BottomRight().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom left"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().BottomLeft().Information("Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().BottomLeft().Success("Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().BottomLeft().Warning("Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().BottomLeft().Error("Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top center with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().TopCenter().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().TopCenter().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().TopCenter().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().TopCenter().Error$1("This is a title", "Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts top full with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().TopFull().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().TopFull().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().TopFull().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().TopFull().Error$1("This is a title", "Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom center with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().BottomCenter().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().BottomCenter().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().BottomCenter().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().BottomCenter().Error$1("This is a title", "Error!");
                })]), Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Toasts bottom full with title"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.UI.Button$1().SetText("Info").OnClick(function (s, e) {
                    tss.UI.Toast().BottomFull().Information$1("This is a title", "Info!");
                }), tss.UI.Button$1().SetText("Success").OnClick(function (s, e) {
                    tss.UI.Toast().BottomFull().Success$1("This is a title", "Success!");
                }), tss.UI.Button$1().SetText("Warning").OnClick(function (s, e) {
                    tss.UI.Toast().BottomFull().Warning$1("This is a title", "Warning!");
                }), tss.UI.Button$1().SetText("Error").OnClick(function (s, e) {
                    tss.UI.Toast().BottomFull().Error$1("This is a title", "Error!");
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ToggleSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Toggles represent a physical switch that allows users to turn things on or off. Use Toggles to present users with two mutually exclusive options (like on/off), where choosing an option results in an immediate action. Use a Toggle for binary operations that take effect right after the user flips the Toggle. For example, use a Toggle to turn services or hardware components on or off. In other words, if a physical switch would work for the action, a Toggle is probably the best control to use."), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Choosing between Toggle and Checkbox")), tss.UI.TextBlock("For some actions, either a Toggle or a Checkbox might work. To decide which control would work better, follow these tips:"), tss.UI.TextBlock("Use a Toggle for binary settings when changes become effective immediately after the user changes them."), tss.UI.TextBlock("In the above example, it's clear with the Toggle that the wireless is set to \"On.\" But with the Checkbox, the user needs to think about whether the wireless is on now or whether they need to check the box to turn wireless on."), tss.UI.TextBlock("Use a Checkbox when the user has to perform extra steps for changes to be effective. For example, if the user must click a \"Submit\", \"Next\", \"Ok\" button to apply changes, use a Checkbox.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Only replace the On and Off labels if there are more specific labels for the setting. If there are short (3-4 characters) labels that represent binary opposites that are more appropriate for a particular setting, use them. ")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t use a Toggle if the user will have to do something else or go somewhere else in order to experience its effect. If any extra step is required for changes to be effective, you should use a checkbox and corresponding \"Apply\" button instead of a Toggle.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic Toggles")), tss.UI.Label$1("Enabled and checked").SetContent(tss.UI.Toggle().Checked()), tss.UI.Label$1("Enabled and unchecked").SetContent(tss.UI.Toggle()), tss.UI.Label$1("Disabled and checked").SetContent(tss.UI.Toggle().Checked().Disabled()), tss.UI.Label$1("Disabled and unchecked").SetContent(tss.UI.Toggle().Disabled()), tss.UI.Label$1("With inline label").Inline().SetContent(tss.UI.Toggle()), tss.txtX.Disabled(tss.Label, tss.UI.Label$1("Disabled with inline label").Inline().SetContent(tss.UI.Toggle().Disabled())), tss.UI.Toggle$1("With inline label and without onText and offText"), tss.UI.Toggle$1("Disabled with inline label and without onText and offText").Disabled()]));
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

                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("TutorialModalSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Tutorial modals are used for processes where the user can be heavily guided, but still needs to enter data."), tss.UI.TextBlock("For usage requiring a quick choice from the user, Dialog may be a more appropriate control.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Use Modals for interactions where something is created and has multiple fields, such as creating a user."), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Always have at least one focusable element inside a Modal.")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Don\u2019t overuse Tutorial Modals. In some cases they can be perceived as interrupting workflow, and too many can be a bad user experience.")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.UI.Button$1("Open Tutorial Modal").OnClick(function (s, e) {
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("UIconsSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("Tesserae integrates the UIcons icons as part of the package, with an auto-generated strongly typed enum for them.")])), tss.ICX.S(tss.S, tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle(System.String.format("enum {0}:", ["UIcons"])), tss.UI.SearchableList(Tesserae.Tests.Samples.UIconsSample.IconItem, ($t = Tesserae.Tests.Samples.UIconsSample.IconItem, System.Linq.Enumerable.from(this.GetAllIcons(), $t).ToArray($t)), [tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25), tss.usX.percent$1(25)])])), true, false, "");
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

                this.content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("ValidatorSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.UI.TextBlock("The validator helper allows you to capture the state of multiple components registered on it.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Best Practices"), tss.ICTX.Children$6(tss.S, tss.UI.HStack(), [tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Do"), Tesserae.Tests.Samples.SamplesHelper.SampleDo("Display useful validation warning messages to components for when the User has left them in an invalid state or when they have tried to submit a partially-populated form")]), tss.ICTX.Children$6(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), [Tesserae.Tests.Samples.SamplesHelper.SampleSubTitle("Don't"), Tesserae.Tests.Samples.SamplesHelper.SampleDont("Display ALL validation warnings messages as soon as a form is rendered, give the User an opportunity to interact with it and enter valid values before shouting at them")])])])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Basic TextBox")), tss.ICTX.Children$6(tss.S, tss.ICX.Padding(tss.S, tss.ICX.Width(tss.S, tss.UI.Stack(), tss.usX.percent$1(40)), tss.usX.px$1(8)), [tss.UI.Label$1("Non-empty").SetContent(textBoxThatMustBeNonEmpty), tss.UI.Label$1("Integer > 0 (must not match the value above)").SetContent(textBoxThatMustBePositiveInteger), tss.UI.Label$1("Pre-filled Integer > 0 (initially valid)").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("123").Required(), tss.Validation.NonZeroPositiveInteger, validator)), tss.UI.Label$1("Pre-filled Integer > 0 (initially i  nvalid)").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("xyz").Required(), tss.Validation.NonZeroPositiveInteger, validator)), tss.UI.Label$1("Not empty with forced instant validation").SetContent(tss.vX.Validation(tss.TextBox, tss.UI.TextBox$1("").Required(), function (tb) {
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
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Section(tss.SectionStackX.Title(tss.UI.SectionStack(), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("VirtualizedListSample")), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Overview"), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("List provides a base component for rendering large sets of items. It is agnostic of the tile component used, and selection management. These concerns can be layered separately."), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.txt, tss.UI.TextBlock("Performance is important, and DOM content is expensive. Therefore, limit what you render. List applies this principle by using UI virtualization. Unlike a simple for loop that renders all items in a set, a List only renders a subset of items, and as you scroll around, the subset of rendered content is shifted. This gives a much better experience for large sets, especially when the per-item components are complex/render-intensive/network-intensive."), tss.usX.px$1(16)), tss.UI.TextBlock("List breaks down the set of items passed in into pages. Only pages within a 'materialized window' are actually rendered. As that window changes due to scroll events, pages that fall outside that window are removed, and their layout space is remembered and pushed into spacer elements. This gives the user the experience of browsing massive amounts of content but only using a small number of actual elements. This gives the browser much less layout to resolve.")])), tss.ICTX.Children$6(tss.S, tss.UI.Stack(), [Tesserae.Tests.Samples.SamplesHelper.SampleTitle("Usage"), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Virtualized List")), tss.usX.px$1(16)), tss.ICX.PaddingBottom(tss.VirtualizedList, tss.UI.VirtualizedList().WithListItems(this.GetALotOfItems()), tss.usX.px$1(32)), tss.ICX.PaddingBottom(tss.txt, tss.ITFX.Medium(tss.txt, tss.UI.TextBlock("Virtualized List with Empty List Message")), tss.usX.px$1(16)), tss.UI.VirtualizedList().WithEmptyMessage(function () {
                    return tss.UI.TextBlock("No List Items");
                }).WithListItems(System.Linq.Enumerable.empty())]));
            }
        },
        methods: {
            Render: function () {
                return this._content.tss$IC$Render();
            },
            GetALotOfItems: function () {
                return System.Linq.Enumerable.range(1, 5000).select(function (number) {
                    return new Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem(System.String.format("Lorem Ipsum {0}", [H5.box(number, System.Int32)]));
                });
            }
        }
    });

    H5.define("Tesserae.Tests.src.Samples.Collections.MasonrySample", {
        inherits: [tss.IC,Tesserae.Tests.ISample],
        fields: {
            _content: null
        },
        alias: ["Render", "tss$IC$Render"],
        ctors: {
            ctor: function () {
                var $t;
                this.$initialize();
                this._content = tss.SectionStackX.Section(tss.SectionStackX.Title(tss.ICX.S(tss.SectionStack, tss.UI.SectionStack()), Tesserae.Tests.Samples.SamplesHelper.SampleHeader("MasonrySample")), tss.ScrollBar.ScrollY(tss.S, tss.ICTX.Children$6(tss.S, tss.ICX.S(tss.S, tss.UI.VStack()), [tss.ICTX.Children$6(tss.Masonry, tss.ICX.S(tss.Masonry, tss.UI.Masonry(4)), ($t = tss.IC, System.Linq.Enumerable.from(this.GetCards(100), $t).ToArray($t)))])), true, false, "");
            }
        },
        methods: {
            GetCards: function (v) {
                return new (H5.GeneratorEnumerable$1(tss.IC))(H5.fn.bind(this, function (v) {
                    var $s = 0,
                        $jff,
                        $rv,
                        rng,
                        i,
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
                                        if ( i < v ) {
                                                $s = 2;
                                                continue;
                                            }
                                        $s = 5;
                                        continue;
                                    }
                                    case 2: {
                                        $en.current = tss.ICX.H(tss.Card, tss.UI.Card(tss.ICTX.Children$6(tss.S, tss.UI.VStack().AlignItemsCenter().JustifyContent("center"), [tss.txtX.NoWrap(tss.txt, tss.UI.TextBlock(System.String.format("Card {0}", [H5.box(i, System.Int32)])))])), ((100 + H5.Int.mul(H5.Int.clip32((rng.NextDouble() * 6)), 50)) | 0));
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

    var $m = H5.setMetadata,
        $n = ["System","Tesserae","System.Collections.Generic","System.Threading.Tasks","Tesserae.Tests.Samples"];
    $m("Tesserae.Tests.App", function () { return {"att":1048960,"a":4,"s":true,"m":[{"a":1,"n":"CenteredCardWithBackground","is":true,"t":8,"pi":[{"n":"content","pt":tss.IC,"ps":0}],"sn":"CenteredCardWithBackground","rt":tss.BackgroundArea,"p":[tss.IC]},{"a":1,"n":"Main","is":true,"t":8,"sn":"Main","rt":$n[0].Void},{"a":1,"n":"_sidebarOpenStateKey","is":true,"t":4,"rt":$n[0].String,"sn":"_sidebarOpenStateKey"},{"a":1,"n":"_sidebarOrderKey","is":true,"t":4,"rt":$n[0].String,"sn":"_sidebarOrderKey"}]}; }, $n);
    $m("Tesserae.Tests.SamplesSourceCode", function () { return {"att":1048576,"a":4,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"GetCodeForSample","is":true,"t":8,"pi":[{"n":"sampleName","pt":$n[0].String,"ps":0}],"sn":"GetCodeForSample","rt":$n[0].String,"p":[$n[0].String]}]}; }, $n);
    $m("Tesserae.Tests.ISample", function () { return {"att":1048737,"a":2}; }, $n);
    $m("Tesserae.Tests.Sample", function () { return {"att":1048576,"a":4,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String,$n[0].String,$n[0].Int32,$n[1].UIcons,Function],"pi":[{"n":"type","pt":$n[0].String,"ps":0},{"n":"name","pt":$n[0].String,"ps":1},{"n":"group","pt":$n[0].String,"ps":2},{"n":"order","pt":$n[0].Int32,"ps":3},{"n":"icon","pt":$n[1].UIcons,"ps":4},{"n":"contentGenerator","pt":Function,"ps":5}],"sn":"ctor"},{"a":2,"n":"ContentGenerator","t":16,"rt":Function,"g":{"a":2,"n":"get_ContentGenerator","t":8,"rt":Function,"fg":"ContentGenerator"},"fn":"ContentGenerator"},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"fn":"Group"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"fn":"Icon"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":2,"n":"Order","t":16,"rt":$n[0].Int32,"g":{"a":2,"n":"get_Order","t":8,"rt":$n[0].Int32,"fg":"Order","box":function ($v) { return H5.box($v, System.Int32);}},"fn":"Order"},{"a":2,"n":"Type","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Type","t":8,"rt":$n[0].String,"fg":"Type"},"fn":"Type"},{"a":1,"backing":true,"n":"<ContentGenerator>k__BackingField","t":4,"rt":Function,"sn":"ContentGenerator"},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"},{"a":1,"backing":true,"n":"<Order>k__BackingField","t":4,"rt":$n[0].Int32,"sn":"Order","box":function ($v) { return H5.box($v, System.Int32);}},{"a":1,"backing":true,"n":"<Type>k__BackingField","t":4,"rt":$n[0].String,"sn":"Type"}]}; }, $n);
    $m("Tesserae.Tests.SampleDetailsAttribute", function () { return {"att":1048577,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"s":{"a":2,"n":"set_Group","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Group"},"fn":"Group"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"s":{"a":2,"n":"set_Icon","t":8,"p":[$n[1].UIcons],"rt":$n[0].Void,"fs":"Icon"},"fn":"Icon"},{"a":2,"n":"Order","t":16,"rt":$n[0].Int32,"g":{"a":2,"n":"get_Order","t":8,"rt":$n[0].Int32,"fg":"Order","box":function ($v) { return H5.box($v, System.Int32);}},"s":{"a":2,"n":"set_Order","t":8,"p":[$n[0].Int32],"rt":$n[0].Void,"fs":"Order"},"fn":"Order"},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Order>k__BackingField","t":4,"rt":$n[0].Int32,"sn":"Order","box":function ($v) { return H5.box($v, System.Int32);}}]}; }, $n);
    $m("Tesserae.Tests.src.Samples.Collections.MasonrySample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 1667
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetCards","t":8,"pi":[{"n":"v","pt":$n[0].Int32,"ps":0}],"sn":"GetCards","rt":$n[2].IEnumerable$1(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.BreadcrumbSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2219
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 3314
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetComponentDetailsListItems","t":8,"sn":"GetComponentDetailsListItems","rt":System.Array.type(Tesserae.Tests.Samples.DetailsListSampleItemWithComponents)},{"a":1,"n":"GetDetailsListItems","t":8,"pi":[{"n":"start","dv":1,"o":true,"pt":$n[0].Int32,"ps":0},{"n":"count","dv":100,"o":true,"pt":$n[0].Int32,"ps":1}],"sn":"GetDetailsListItems","rt":System.Array.type(Tesserae.Tests.Samples.DetailsListSampleFileItem),"p":[$n[0].Int32,$n[0].Int32]},{"a":1,"n":"GetDetailsListItemsAsync","t":8,"pi":[{"n":"start","dv":1,"o":true,"pt":$n[0].Int32,"ps":0},{"n":"count","dv":100,"o":true,"pt":$n[0].Int32,"ps":1}],"sn":"GetDetailsListItemsAsync","rt":$n[3].Task$1(System.Array.type(Tesserae.Tests.Samples.DetailsListSampleFileItem)),"p":[$n[0].Int32,$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSampleFileItem", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].UIcons,$n[0].String,$n[0].DateTime,$n[0].String,$n[0].Double],"pi":[{"n":"fileIcon","pt":$n[1].UIcons,"ps":0},{"n":"fileName","pt":$n[0].String,"ps":1},{"n":"dateModified","pt":$n[0].DateTime,"ps":2},{"n":"modifiedBy","pt":$n[0].String,"ps":3},{"n":"fileSize","pt":$n[0].Double,"ps":4}],"sn":"ctor"},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[4].DetailsListSampleFileItem,"ps":0},{"n":"columnSortingKey","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[4].DetailsListSampleFileItem,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"listItemIndex","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[2].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"createGridCellExpression","pt":Function,"ps":1}],"sn":"Render","rt":$n[2].IEnumerable$1(tss.IC),"p":[$n[2].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"DateModified","t":16,"rt":$n[0].DateTime,"g":{"a":2,"n":"get_DateModified","t":8,"rt":$n[0].DateTime,"fg":"DateModified","box":function ($v) { return H5.box($v, System.DateTime, System.DateTime.format);}},"fn":"DateModified"},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"FileIcon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_FileIcon","t":8,"rt":$n[1].UIcons,"fg":"FileIcon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"fn":"FileIcon"},{"a":2,"n":"FileName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_FileName","t":8,"rt":$n[0].String,"fg":"FileName"},"fn":"FileName"},{"a":2,"n":"FileSize","t":16,"rt":$n[0].Double,"g":{"a":2,"n":"get_FileSize","t":8,"rt":$n[0].Double,"fg":"FileSize","box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},"fn":"FileSize"},{"a":2,"n":"ModifiedBy","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ModifiedBy","t":8,"rt":$n[0].String,"fg":"ModifiedBy"},"fn":"ModifiedBy"},{"a":1,"backing":true,"n":"<DateModified>k__BackingField","t":4,"rt":$n[0].DateTime,"sn":"DateModified","box":function ($v) { return H5.box($v, System.DateTime, System.DateTime.format);}},{"a":1,"backing":true,"n":"<FileIcon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"FileIcon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<FileName>k__BackingField","t":4,"rt":$n[0].String,"sn":"FileName"},{"a":1,"backing":true,"n":"<FileSize>k__BackingField","t":4,"rt":$n[0].Double,"sn":"FileSize","box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},{"a":1,"backing":true,"n":"<ModifiedBy>k__BackingField","t":4,"rt":$n[0].String,"sn":"ModifiedBy"}]}; }, $n);
    $m("Tesserae.Tests.Samples.DetailsListSampleItemWithComponents", function () { return {"att":1048577,"a":2,"m":[{"a":2,"isSynthetic":true,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[4].DetailsListSampleItemWithComponents,"ps":0},{"n":"columnSortingKey","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[4].DetailsListSampleItemWithComponents,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"listItemIndex","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[2].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"createGridCellExpression","pt":Function,"ps":1}],"sn":"Render","rt":$n[2].IEnumerable$1(tss.IC),"p":[$n[2].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"WithButton","t":8,"pi":[{"n":"button","pt":tss.Button,"ps":0}],"sn":"WithButton","rt":$n[4].DetailsListSampleItemWithComponents,"p":[tss.Button]},{"a":2,"n":"WithCheckBox","t":8,"pi":[{"n":"checkBox","pt":tss.ChecBox,"ps":0}],"sn":"WithCheckBox","rt":$n[4].DetailsListSampleItemWithComponents,"p":[tss.ChecBox]},{"a":2,"n":"WithChoiceGroup","t":8,"pi":[{"n":"choiceGroup","pt":tss.ChoiceGroup,"ps":0}],"sn":"WithChoiceGroup","rt":$n[4].DetailsListSampleItemWithComponents,"p":[tss.ChoiceGroup]},{"a":2,"n":"WithDropdown","t":8,"pi":[{"n":"dropdown","pt":tss.Dropdown,"ps":0}],"sn":"WithDropdown","rt":$n[4].DetailsListSampleItemWithComponents,"p":[tss.Dropdown]},{"a":2,"n":"WithIcon","t":8,"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0}],"sn":"WithIcon","rt":$n[4].DetailsListSampleItemWithComponents,"p":[$n[1].UIcons]},{"a":2,"n":"WithName","t":8,"pi":[{"n":"name","pt":$n[0].String,"ps":0}],"sn":"WithName","rt":$n[4].DetailsListSampleItemWithComponents,"p":[$n[0].String]},{"a":2,"n":"WithToggle","t":8,"pi":[{"n":"toggle","pt":tss.Toggle,"ps":0}],"sn":"WithToggle","rt":$n[4].DetailsListSampleItemWithComponents,"p":[tss.Toggle]},{"a":2,"n":"Button","t":16,"rt":tss.Button,"g":{"a":2,"n":"get_Button","t":8,"rt":tss.Button,"fg":"Button"},"s":{"a":1,"n":"set_Button","t":8,"p":[tss.Button],"rt":$n[0].Void,"fs":"Button"},"fn":"Button"},{"a":2,"n":"CheckBox","t":16,"rt":tss.ChecBox,"g":{"a":2,"n":"get_CheckBox","t":8,"rt":tss.ChecBox,"fg":"CheckBox"},"s":{"a":1,"n":"set_CheckBox","t":8,"p":[tss.ChecBox],"rt":$n[0].Void,"fs":"CheckBox"},"fn":"CheckBox"},{"a":2,"n":"ChoiceGroup","t":16,"rt":tss.ChoiceGroup,"g":{"a":2,"n":"get_ChoiceGroup","t":8,"rt":tss.ChoiceGroup,"fg":"ChoiceGroup"},"s":{"a":1,"n":"set_ChoiceGroup","t":8,"p":[tss.ChoiceGroup],"rt":$n[0].Void,"fs":"ChoiceGroup"},"fn":"ChoiceGroup"},{"a":2,"n":"Dropdown","t":16,"rt":tss.Dropdown,"g":{"a":2,"n":"get_Dropdown","t":8,"rt":tss.Dropdown,"fg":"Dropdown"},"s":{"a":1,"n":"set_Dropdown","t":8,"p":[tss.Dropdown],"rt":$n[0].Void,"fs":"Dropdown"},"fn":"Dropdown"},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"Icon","t":16,"rt":$n[1].UIcons,"g":{"a":2,"n":"get_Icon","t":8,"rt":$n[1].UIcons,"fg":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},"s":{"a":1,"n":"set_Icon","t":8,"p":[$n[1].UIcons],"rt":$n[0].Void,"fs":"Icon"},"fn":"Icon"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"s":{"a":1,"n":"set_Name","t":8,"p":[$n[0].String],"rt":$n[0].Void,"fs":"Name"},"fn":"Name"},{"a":2,"n":"Toggle","t":16,"rt":tss.Toggle,"g":{"a":2,"n":"get_Toggle","t":8,"rt":tss.Toggle,"fg":"Toggle"},"s":{"a":1,"n":"set_Toggle","t":8,"p":[tss.Toggle],"rt":$n[0].Void,"fs":"Toggle"},"fn":"Toggle"},{"a":1,"backing":true,"n":"<Button>k__BackingField","t":4,"rt":tss.Button,"sn":"Button"},{"a":1,"backing":true,"n":"<CheckBox>k__BackingField","t":4,"rt":tss.ChecBox,"sn":"CheckBox"},{"a":1,"backing":true,"n":"<ChoiceGroup>k__BackingField","t":4,"rt":tss.ChoiceGroup,"sn":"ChoiceGroup"},{"a":1,"backing":true,"n":"<Dropdown>k__BackingField","t":4,"rt":tss.Dropdown,"sn":"Dropdown"},{"a":1,"backing":true,"n":"<Icon>k__BackingField","t":4,"rt":$n[1].UIcons,"sn":"Icon","box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"},{"a":1,"backing":true,"n":"<Toggle>k__BackingField","t":4,"rt":tss.Toggle,"sn":"Toggle"}]}; }, $n);
    $m("Tesserae.Tests.Samples.InfiniteScrollingListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 1916
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"page","dv":-1,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"txt","dv":"","o":true,"pt":$n[0].String,"ps":2}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32,$n[0].Int32,$n[0].String]},{"a":1,"n":"GetSomeItemsAsync","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0},{"n":"page","dv":-1,"o":true,"pt":$n[0].Int32,"ps":1},{"n":"txt","dv":"","o":true,"pt":$n[0].String,"ps":2}],"sn":"GetSomeItemsAsync","rt":$n[3].Task$1(System.Array.type(tss.IC)),"p":[$n[0].Int32,$n[0].Int32,$n[0].String]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ItemsListSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2111
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.OverflowSetSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 10, Icon: 2219
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableGroupedListSample", function () { return {"nested":[$n[4].SearchableGroupedListSample.SearchableGroupedListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2907
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetItems","rt":System.Array.type(Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableGroupedListSample.SearchableGroupedListItem", function () { return {"td":$n[4].SearchableGroupedListSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[0].String],"pi":[{"n":"value","pt":$n[0].String,"ps":0},{"n":"group","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"Group","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Group","t":8,"rt":$n[0].String,"fg":"Group"},"fn":"Group"},{"a":1,"n":"_component","t":4,"rt":tss.IC,"sn":"_component","ro":true},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"backing":true,"n":"<Group>k__BackingField","t":4,"rt":$n[0].String,"sn":"Group"}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableListSample", function () { return {"nested":[$n[4].SearchableListSample.SearchableListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2907
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetItems","rt":System.Array.type(Tesserae.Tests.Samples.SearchableListSample.SearchableListItem),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchableListSample.SearchableListItem", function () { return {"td":$n[4].SearchableListSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"value","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_component","t":4,"rt":tss.IC,"sn":"_component","ro":true},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.StackSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 0, Icon: 2841
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.S,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.S,$n[0].Int32]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true},{"a":1,"n":"sampleSortableStackLocalStorageKey","is":true,"t":4,"rt":$n[0].String,"sn":"sampleSortableStackLocalStorageKey"}]}; }, $n);
    $m("Tesserae.Tests.Samples.TimelineSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 3424
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.VirtualizedListSample", function () { return {"nested":[$n[4].VirtualizedListSample.SampleVirtualizedItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Collections", Order: 20, Icon: 2111
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetALotOfItems","t":8,"sn":"GetALotOfItems","rt":$n[2].IEnumerable$1(Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.VirtualizedListSample.SampleVirtualizedItem", function () { return {"td":$n[4].VirtualizedListSample,"att":1048834,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_innerElement","t":4,"rt":HTMLElement,"sn":"_innerElement","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ActionButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1078
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ButtonSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1078
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.CheckBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 745
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ChoiceGroupSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2111
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ColorPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 2433
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DatePickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 566
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DateTimePickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 566
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DropdownSample", function () { return {"att":1048833,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 661
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetAsync","is":true,"t":8,"pi":[{"n":"url","pt":$n[0].String,"ps":0}],"sn":"GetAsync","rt":$n[3].Task$1(System.String),"p":[$n[0].String]},{"a":1,"n":"GetGoogleItemsAsync","t":8,"sn":"GetGoogleItemsAsync","rt":$n[3].Task$1(System.Array.type(tss.Dropdown.Item))},{"a":1,"n":"GetItemsAsync","t":8,"sn":"GetItemsAsync","rt":$n[3].Task$1(System.Array.type(tss.Dropdown.Item))},{"a":1,"n":"GetZeroItemsAsync","t":8,"sn":"GetZeroItemsAsync","rt":$n[3].Task$1(System.Array.type(tss.Dropdown.Item))},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"StartLoadingAsyncDataImmediately","is":true,"t":8,"pi":[{"n":"dropdown","pt":tss.Dropdown,"ps":0}],"sn":"StartLoadingAsyncDataImmediately","rt":tss.Dropdown,"p":[tss.Dropdown]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EditableLabelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 1277
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.GridPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 3310
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetGameOfLifeSample","is":true,"t":8,"sn":"GetGameOfLifeSample","rt":tss.S},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.GridSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 1667
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.HorizontalSeparatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 1836
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.LabelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 3382
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.NumberPickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 1921
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 10, Icon: 661
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetComponentPickerItems","t":8,"sn":"GetComponentPickerItems","rt":System.Array.type(Tesserae.Tests.Samples.PickerSampleItemWithComponents)},{"a":1,"n":"GetPickerItems","t":8,"sn":"GetPickerItems","rt":System.Array.type(Tesserae.Tests.Samples.PickerSampleItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSampleItem", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"name","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"IsSelected","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_IsSelected","t":8,"rt":$n[0].Boolean,"fg":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"s":{"a":2,"n":"set_IsSelected","t":8,"p":[$n[0].Boolean],"rt":$n[0].Void,"fs":"IsSelected"},"fn":"IsSelected"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":1,"backing":true,"n":"<IsSelected>k__BackingField","t":4,"rt":$n[0].Boolean,"sn":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.PickerSampleItemWithComponents", function () { return {"att":1048577,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String,$n[1].UIcons],"pi":[{"n":"name","pt":$n[0].String,"ps":0},{"n":"icon","pt":$n[1].UIcons,"ps":1}],"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":2,"n":"IsSelected","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_IsSelected","t":8,"rt":$n[0].Boolean,"fg":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"s":{"a":2,"n":"set_IsSelected","t":8,"p":[$n[0].Boolean],"rt":$n[0].Void,"fs":"IsSelected"},"fn":"IsSelected"},{"a":2,"n":"Name","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_Name","t":8,"rt":$n[0].String,"fg":"Name"},"fn":"Name"},{"a":1,"n":"_icon","t":4,"rt":$n[1].UIcons,"sn":"_icon","ro":true,"box":function ($v) { return H5.box($v, Tesserae.UIcons, System.Enum.toStringFn(Tesserae.UIcons));}},{"a":1,"backing":true,"n":"<IsSelected>k__BackingField","t":4,"rt":$n[0].Boolean,"sn":"IsSelected","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":1,"backing":true,"n":"<Name>k__BackingField","t":4,"rt":$n[0].String,"sn":"Name"}]}; }, $n);
    $m("Tesserae.Tests.Samples.SearchBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2907
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SliderSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2936
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SplitViewSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 3309
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBlockSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1346
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBoxSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 1923
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TextBreadcrumbsSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 20, Icon: 2219
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ToggleSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Components", Order: 0, Icon: 2936
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ProgressIndicatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 10, Icon: 283
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ProgressModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 20, Icon: 3762
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SpinnerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Progress", Order: 10, Icon: 3132
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SamplesHelper", function () { return {"att":1048961,"a":2,"s":true,"m":[{"a":2,"n":"SampleDo","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleDo","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleDont","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleDont","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleHeader","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].String,"ps":0}],"sn":"SampleHeader","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleSubTitle","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleSubTitle","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"SampleTitle","is":true,"t":8,"pi":[{"n":"text","pt":$n[0].String,"ps":0}],"sn":"SampleTitle","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"ShowSampleCode","is":true,"t":8,"pi":[{"n":"sampleType","pt":$n[0].String,"ps":0}],"sn":"ShowSampleCode","rt":$n[0].Void,"p":[$n[0].String]}]}; }, $n);
    $m("Tesserae.Tests.Samples.ContextMenuSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 108
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DialogSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 3763
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.FloatSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 0, Icon: 1603
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.LayerSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 0, Icon: 3313
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 3764
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PanelSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 10, Icon: 3763
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.PivotSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 3310
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetSomeItems","t":8,"pi":[{"n":"count","pt":$n[0].Int32,"ps":0}],"sn":"GetSomeItems","rt":System.Array.type(tss.IC),"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.SectionStackSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 416
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.SectionStack,$n[0].Int32]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.TutorialModalSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Surfaces", Order: 20, Icon: 1912
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SampleTutorialModal","is":true,"t":8,"sn":"SampleTutorialModal","rt":tss.TutorialModal},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.DeferSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 3132
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"SetChildren","t":8,"pi":[{"n":"stack","pt":tss.SectionStack,"ps":0},{"n":"count","pt":$n[0].Int32,"ps":1}],"sn":"SetChildren","rt":$n[0].Void,"p":[tss.SectionStack,$n[0].Int32]},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EmojiSample", function () { return {"nested":[$n[4].EmojiSample.IconItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 3058
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetAllIcons","t":8,"sn":"GetAllIcons","rt":$n[2].IEnumerable$1(Tesserae.Tests.Samples.EmojiSample.IconItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ToValidName","is":true,"t":8,"pi":[{"n":"icon","pt":$n[0].String,"ps":0}],"sn":"ToValidName","rt":$n[0].String,"p":[$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.EmojiSample.IconItem", function () { return {"td":$n[4].EmojiSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].Emoji,$n[0].String],"pi":[{"n":"icon","pt":$n[1].Emoji,"ps":0},{"n":"name","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"n":"component","t":4,"rt":tss.IC,"sn":"component","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.FileSelectorAndDropAreaSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 20, Icon: 1544
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ThemeColorsSample", function () { return {"nested":[$n[4].ThemeColorsSample.ColorListItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 2433
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"DumpTheme","is":true,"t":8,"sn":"DumpTheme","rt":$n[0].Void},{"a":2,"n":"LightDiff","is":true,"t":8,"pi":[{"n":"from","pt":$n[0].String,"ps":0},{"n":"to","pt":$n[0].String,"ps":1}],"sn":"LightDiff","rt":$n[0].Double,"p":[$n[0].String,$n[0].String],"box":function ($v) { return H5.box($v, System.Double, System.Double.format, System.Double.getHashCode);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content"}]}; }, $n);
    $m("Tesserae.Tests.Samples.ThemeColorsSample.ColorListItem", function () { return {"td":$n[4].ThemeColorsSample,"att":1048578,"a":2,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[0].String],"pi":[{"n":"themeName","pt":$n[0].String,"ps":0}],"sn":"ctor"},{"a":1,"n":"ColorSquare","is":true,"t":8,"pi":[{"n":"color","pt":$n[0].String,"ps":0}],"sn":"ColorSquare","rt":tss.IC,"p":[$n[0].String]},{"a":2,"n":"CompareTo","t":8,"pi":[{"n":"other","pt":$n[4].ThemeColorsSample.ColorListItem,"ps":0},{"n":"columnSortingKey","pt":$n[0].String,"ps":1}],"sn":"CompareTo","rt":$n[0].Int32,"p":[$n[4].ThemeColorsSample.ColorListItem,$n[0].String],"box":function ($v) { return H5.box($v, System.Int32);}},{"a":2,"n":"OnListItemClick","t":8,"pi":[{"n":"listItemIndex","pt":$n[0].Int32,"ps":0}],"sn":"OnListItemClick","rt":$n[0].Void,"p":[$n[0].Int32]},{"a":2,"n":"Render","t":8,"pi":[{"n":"columns","pt":$n[2].IList$1(tss.IDetailsListColumn),"ps":0},{"n":"createGridCellExpression","pt":Function,"ps":1}],"sn":"Render","rt":$n[2].IEnumerable$1(tss.IC),"p":[$n[2].IList$1(tss.IDetailsListColumn),Function]},{"a":2,"n":"EnableOnListItemClickEvent","t":16,"rt":$n[0].Boolean,"g":{"a":2,"n":"get_EnableOnListItemClickEvent","t":8,"rt":$n[0].Boolean,"fg":"EnableOnListItemClickEvent","box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},"fn":"EnableOnListItemClickEvent"},{"a":2,"n":"ThemeName","t":16,"rt":$n[0].String,"g":{"a":2,"n":"get_ThemeName","t":8,"rt":$n[0].String,"fg":"ThemeName"},"fn":"ThemeName"},{"a":2,"n":"Mapping","is":true,"t":4,"rt":$n[2].Dictionary$2(System.String,System.Collections.Generic.Dictionary$2(System.String,System.String)),"sn":"Mapping"},{"a":1,"backing":true,"n":"<ThemeName>k__BackingField","t":4,"rt":$n[0].String,"sn":"ThemeName"}]}; }, $n);
    $m("Tesserae.Tests.Samples.TippySample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 980
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ToastSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 20, Icon: 501
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.UIconsSample", function () { return {"nested":[$n[4].UIconsSample.IconItem],"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 10, Icon: 2576
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":1,"n":"GetAllIcons","t":8,"sn":"GetAllIcons","rt":$n[2].IEnumerable$1(Tesserae.Tests.Samples.UIconsSample.IconItem)},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"ToValidName","is":true,"t":8,"pi":[{"n":"icon","pt":$n[0].String,"ps":0}],"sn":"ToValidName","rt":$n[0].String,"p":[$n[0].String]},{"a":1,"n":"_content","t":4,"rt":tss.IC,"sn":"_content","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.UIconsSample.IconItem", function () { return {"td":$n[4].UIconsSample,"att":1048579,"a":1,"m":[{"a":2,"n":".ctor","t":1,"p":[$n[1].UIcons,$n[0].String],"pi":[{"n":"icon","pt":$n[1].UIcons,"ps":0},{"n":"name","pt":$n[0].String,"ps":1}],"sn":"ctor"},{"a":2,"n":"IsMatch","t":8,"pi":[{"n":"searchTerm","pt":$n[0].String,"ps":0}],"sn":"IsMatch","rt":$n[0].Boolean,"p":[$n[0].String],"box":function ($v) { return H5.box($v, System.Boolean, System.Boolean.toString);}},{"a":2,"n":"Render","t":8,"sn":"Render","rt":tss.IC},{"a":1,"n":"_value","t":4,"rt":$n[0].String,"sn":"_value","ro":true},{"a":1,"n":"component","t":4,"rt":tss.IC,"sn":"component","ro":true}]}; }, $n);
    $m("Tesserae.Tests.Samples.ValidatorSample", function () { return {"att":1048577,"a":2,"at":[H5.apply(new Tesserae.Tests.SampleDetailsAttribute(), {
        Group: "Utilities", Order: 0, Icon: 740
    } )],"m":[{"a":2,"n":".ctor","t":1,"sn":"ctor"},{"a":2,"n":"Render","t":8,"sn":"Render","rt":HTMLElement},{"a":1,"n":"content","t":4,"rt":tss.IC,"sn":"content","ro":true}]}; }, $n);
});
