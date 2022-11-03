{
    const IsEmptyOrSpaces = function (str) {
        return str === null || str.match(/^ *$/) !== null;
    }

    const UI_EXPAND_BUTTON_CLASS_NAME = "rwui_expandable_item_title rwui_expand";
    const UI_EXPAND_BUTTON_ID = "Other tasks progress info";
    const SCRIPT_INDEX = document.getElementsByTagName('script').length - 1;
    const SCRIPT_PARENT = document.getElementsByTagName('script')[SCRIPT_INDEX].parentElement;
    const COLUMN_CHART_STYLE_CSS = '.huy_td1-column-chart-container { font-family: Calibri !important; position: relative; border-radius: 25px; box-sizing: border-box; box-shadow: 0 3px 6px 0 rgba(0, 0, 0, 0.2), 0 3px 6px 0 rgba(0, 0, 0, 0.19); border: 5px solid #6f6f6f; height: 600px; width: 800px; display: flex; } .huy_td1-column-chart-container:hover { box-shadow: 0 10px 16px 0 rgb(0 0 0 / 20%), 0 6px 20px 0 rgb(0 0 0 / 19%) !important; /* border: 0px solid #6f6f6f; */ } .huy_td1-column-chart-setting-container { display: grid; grid-template-columns: 2% 68% 0.1% 29.9%; background-color: transparent; padding: 10px; width: 100%; } .huy_td1-column-chart-setting-buton { display: inline-block; vertical-align: top; line-height: 0; padding: 0; background-color: transparent; border: 0px; width: 2rem; height: 2rem; z-index: 1; position: absolute; font-family: Calibri; top: 0%; left: 94%; } .huy_td1-column-chart-setting-buton:hover svg { fill: #2c974b; cursor: pointer; } .huy_td1-column-chart-setting-buton:active svg { fill: #257a3d; } .huy_td1-column-chart-main-chart-container { display: grid; grid-template-columns: 2% 88% 2% 8%; width: 100%; height: 100%; } .huy_td1-column-chart-main-chart { grid-column: 2; } /*#region Loader*/ .huy_td1-loader-container { font-family: Calibri !important; width: 100%; height: 100%; display: flex; justify-content: center; align-items: center; } .huy_td1-loader1 { border: 5px solid #f3f3f3; border-radius: 50%; border-top: 5px black solid; width: 50px; height: 50px; animation: spin 2s linear infinite; } /*#endregion Loader*/ /*#region Animation*/ /* Safari */ @-webkit-keyframes spin { 0% { -webkit-transform: rotate(0deg); } 100% { -webkit-transform: rotate(360deg); } } @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } } /*#endregion Animation*/ /*#region Button*/ .huy_td1-button { appearance: none; background-color: #222322; border-radius: 6px; box-sizing: border-box; color: #fff; text-decoration: none; touch-action: manipulation; white-space: nowrap; cursor: pointer; font-family: Calibri; font-size: 10px; line-height: 20px; font-weight: 600; border: 1px solid rgba(27, 31, 35, .15); } .huy_td1-button:hover { background-color: #2c974b; } .huy_td1-button:active { background-color: #298e46; } /*#endregion Button*/ /*#region TextInput*/ .huy_td1-input-field-container { display: grid; background-color: transparent; font-family: Calibri; } .huy_td1-input-field-container .huy_td1-input-maininput:focus { outline: none; border-radius: 5px; border: 2px solid black; } .huy_td1-input-field-container .huy_td1-input-maininput { visibility: visible; -webkit-text-size-adjust: 100%; font-family: Calibri; overflow: visible; box-sizing: border-box; border: 2px solid #ccc; width: 100%; border-radius: 5px; height: 50px; background-color: transparent; grid-row: 1; grid-column: 1; } .huy_td1-input-field-container .huy_td1-input-maininput:valid+.huy_td1-input-placeholder { display: none; } .huy_td1-input-field-container .huy_td1-input-placeholder { visibility: visible; grid-row: 1; grid-column: 1; align-self: center; margin: 0px 5px; font-weight: 400; color: #b4b4b4; pointer-events: none; } /*#endregion TextInput*/ /*#region MultiChoiceInput*/ .huy_td1-multi-choices-input-container { font-weight: 400; min-height: 50px; line-height: 1; color-scheme: light; position: relative; display: inline-block; zoom: 1; font-size: 15px; transition: background-color .2s ease-in-out, border-color .2s ease-in-out; border-radius: 3.01px; box-sizing: border-box; margin: 0; border: 2px solid #ccc; height: auto; padding: 0; } .huy_td1-multi-choices-input-container:has(.huy_td1-multi-choices-input-maininput:focus) { border: 2px solid rgb(0, 0, 0); } .huy_td1-multi-choices-input-listview { letter-spacing: 0; color-scheme: light; font-family: inherit; height: auto !important; display: inline-block; position: relative; cursor: text; overflow: hidden; min-height: 0; border-radius: 3.01px; box-sizing: border-box; margin: 0; margin-top: 6px; line-height: 1.42857143; padding: 3px 4px; max-width: none; padding-right: 16px; pointer-events: none; } .huy_td1-multi-choices-input-listview-item { letter-spacing: 0; color-scheme: light; font-family: inherit; float: left; cursor: default; background: #c0c0c0; box-sizing: content-box; border-radius: 3px; box-shadow: none; display: inline-block; font-weight: 700; padding: 2px 5px 2px 5px; position: relative; margin: 1px 5px 1px 0; text-align: left; text-decoration: none; filter: none; pointer-events: all; } .huy_td1-multi-choices-input-listview-item:hover { background: #6acf99; } .huy_td1-multi-choices-input-search-field { font-weight: 400; line-height: 1; letter-spacing: 0; color-scheme: light; font-family: inherit; float: left; list-style: none; margin: 0; padding: 0; white-space: nowrap; padding: 2px 5px 2px 0; margin: 1px 5px 1px 0; } .huy_td1-multi-choices-input-maininput { list-style: none; white-space: nowrap; color: inherit !important; outline: #6b778c; border: none; font-family: inherit; height: 1.42857143em; line-height: 1.42857143; margin: 0; font-weight: 500; max-width: 280px; min-width: 10px; padding: 0 3px; vertical-align: middle; } .label-choice { display: inline-block; margin-right: 2px; max-width: 200px; overflow: hidden; margin-right: 5px; text-overflow: ellipsis; vertical-align: middle; white-space: nowrap; } .btn-delete-choice { display: inline-block; line-height: 0; border: 0; vertical-align: middle; padding: 0; height: 10px; width: 10px; box-sizing: border-box; background-color: transparent; border-radius: 3px; padding: 1px; border: rgb(118, 118, 118) solid 1px; } .btn-delete-choice:hover { cursor: pointer; background-color: rgb(77, 77, 77); border: none; } .btn-delete-choice:hover svg { fill: rgba(113, 235, 202, 0.592); } /*#endregion MultiChoiceInput*/ /*#region SelectionChooser*/ .huy_td1-selection-chooser-panel { font-family: Calibri; visibility: visible; width: 100%; display: inline-block; height: fit-content; } .huy_td1-selection-chooser-option-container { font-family: Calibri; visibility: visible; min-height: 30px; } .huy_td1-selection-chooser-option-container input[type="radio"] { display: none; } .huy_td1-selection-chooser-option-container input[type="radio"]+div { height: 30px; padding: 5px 7px 5px 5px; line-height: 30px; border-radius: 10px; display: table; } .huy_td1-selection-chooser-option-container input[type="radio"]:checked+div { background: #333; } .huy_td1-selection-chooser-option-container input[type="radio"]+div label { font-weight: 400; font-size: 14px; cursor: pointer; } .huy_td1-selection-chooser-option-container input[type="radio"]:checked+div label { color: white; font-weight: 700; } .huy_td1-selection-chooser-option-container input[type="radio"]:checked+div label span { background-color: #2c974b; border: 2px solid #ffffff; box-shadow: 2px 2px 2px rgb(0 0 0 / 10%); } .huy_td1-selection-chooser-option-container input[type="radio"]+div label span { display: inline-block; width: 18px; height: 18px; margin: -2px 10px 0 0; cursor: pointer; vertical-align: middle; -moz-border-radius: 50%; border-radius: 50%; background-color: #cacaca; border: 2px solid #ffffff; box-shadow: 2px 2px 2px rgb(0 0 0 / 10%); -webkit-transition: background-color 0.24s linear; -o-transition: background-color 0.24s linear; -moz-transition: background-color 0.24s linear; transition: background-color 0.24s linear; } /*#endregion SelectionChooser*/';

    function onChartCreate() {
        let chartStyleEle = document.createElement('style');
        chartStyleEle.innerText = COLUMN_CHART_STYLE_CSS;
        document.head.appendChild(chartStyleEle);

        let script = document.createElement('script');
        script.src = "https://www.gstatic.com/charts/loader.js";
        script.type = "text/javascript";
        script.onload = onGoogleChartScriptLoaded;
        document.head.appendChild(script);
    }

    function onGoogleChartScriptLoaded() {
        google.charts.load("current", { packages: ['corechart'] });
        google.charts.setOnLoadCallback(onGoogleChartLoaded);
    }

    async function onGoogleChartLoaded() {

        const userDataManager = new UserDataManager(SCRIPT_INDEX);
        const columnChartViewManager = new ColumnChartViewManager();

        function initDefaultSettingValueFromUserData() {
            columnChartViewManager.columnChartSettingViewManager.filterItemsList = userDataManager.userData?.filterItems;
            columnChartViewManager.columnChartSettingViewManager.uiExpandButtonIDMainInput.value = userDataManager.userData?.uiExpandBtnID;
            columnChartViewManager.columnChartSettingViewManager.tableColumnIDMainInput.value = userDataManager.userData?.tableColumnID;
            columnChartViewManager.columnChartSettingViewManager.xAxisNameMainInput.value = userDataManager.userData?.xAxisName;
            columnChartViewManager.columnChartSettingViewManager.yAxisNameMainInput.value = userDataManager.userData?.yAxisName;
            userDataManager.userData?.filterItems?.forEach(ele => columnChartViewManager.columnChartSettingViewManager.insertElementForMultiChoices(ele));
        }

        function initChartContentFromUserData() {
            columnChartViewManager.setDataRetrievalOptionFromUserData(userDataManager.userData);
            if (columnChartViewManager.dataRetrievalOption.option == ColumnChartViewManager.DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION) {
                reloadBarChart();
            }
        }

        async function reloadBarChart() {
            const crawlingMess = { message: 0, };
            /**
             *  @param{DataRetrievalOption} dataRetrievalOption
             *  @param{crawlingMess} dataCrawlingMessage
             * **/
            async function loadRawData(dataRetrievalOption, dataCrawlingMessage) {
                function getRandomInt(max) {
                    return Math.floor(Math.random() * max);
                }
                await new Promise(r => setTimeout(r, 100));
                switch (dataRetrievalOption.option) {
                    case ColumnChartViewManager.DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION: {
                        dataCrawlingMessage.message = ColumnChartViewManager.GET_DATA_FAIL_MESSAGE;
                        var uiButtonId = dataRetrievalOption.param.uiExpandBtnID == '' ? UI_EXPAND_BUTTON_ID
                            : dataRetrievalOption.param.uiExpandBtnID;
                        var uiExpandSource = document.getElementsByClassName(UI_EXPAND_BUTTON_CLASS_NAME);
                        var selectedUiExpandBtn = null;
                        for (var i = 0; i < uiExpandSource.length; i++) {
                            var buttonTitle = uiExpandSource[i].innerText;
                            if (buttonTitle.indexOf(uiButtonId) >= 0) {
                                selectedUiExpandBtn = uiExpandSource[i].parentElement;
                                break;
                            }
                        }

                        if (selectedUiExpandBtn != null && selectedUiExpandBtn.getElementsByTagName("table")[0] != null) {
                            var rawTable = selectedUiExpandBtn.getElementsByTagName("table")[0];
                            var tableHeaderRow = rawTable?.getElementsByTagName("thead")[0]?.getElementsByTagName("tr")[0];
                            var tableHeaderRowCells = tableHeaderRow?.getElementsByTagName("th");

                            var columnIndex = -1;
                            for (var i = 0; i < tableHeaderRowCells?.length; i++) {
                                if (tableHeaderRowCells[i]?.innerText == dataRetrievalOption?.param.tableColumnID) {
                                    columnIndex = i;
                                }
                            }

                            if (columnIndex != -1) {
                                let xAxisName = dataRetrievalOption.param.xAxisName;
                                let yAxisName = dataRetrievalOption.param.yAxisName;;
                                let chartData = new ChartData(
                                    IsEmptyOrSpaces(xAxisName) ? "X Axis" : xAxisName
                                    , IsEmptyOrSpaces(yAxisName) ? "Y Axis" : yAxisName);
                                var filterItemsList = dataRetrievalOption?.param?.filterItems;
                                var data = filterItemsList?.map(function (e1) {
                                    let res = Array.from(rawTable.rows).filter(function (e2) {
                                        return e2.cells[columnIndex].innerText.indexOf(e1) >= 0;
                                    });
                                    chartData.addItemToChartData(e1, res.length);
                                    return res.length;
                                });
                                dataCrawlingMessage.message = ColumnChartViewManager.GET_DATA_SUCESSFULLY_MESSAGE;
                                return chartData;
                            }

                        } else if (selectedUiExpandBtn != null && selectedUiExpandBtn.getElementsByTagName("table")[0] == null) {
                            dataCrawlingMessage.message = ColumnChartViewManager.GET_DATA_FAIL_MESSAGE;
                        } else if (selectedUiExpandBtn == null) {
                            dataCrawlingMessage.message = ColumnChartViewManager.GET_DATA_FAIL_MESSAGE;
                        }
                        break;
                    }
                    case ColumnChartViewManager.DATA_RETRIEVAL_DEFAULT_OPTION: {
                        dataCrawlingMessage.message = ColumnChartViewManager.GET_DATA_SUCESSFULLY_MESSAGE;
                        let xAxisName = dataRetrievalOption.param.xAxisName;
                        let yAxisName = dataRetrievalOption.param.yAxisName;
                        let chartData = new ChartData(
                            IsEmptyOrSpaces(xAxisName) ? "X Axis" : xAxisName
                            , IsEmptyOrSpaces(yAxisName) ? "Y Axis" : yAxisName);
                        chartData.addItemToChartData("a", getRandomInt(1000));
                        chartData.addItemToChartData("b", getRandomInt(1000));
                        chartData.addItemToChartData("c", getRandomInt(1000));
                        return chartData;
                    }
                }
            }

            columnChartViewManager.setChartStatus(ColumnChartViewManager.LOADING_STATUS);
            await loadRawData(columnChartViewManager.dataRetrievalOption, crawlingMess).then(
                function (value) {
                    columnChartViewManager.updateChartViewByContent(value, crawlingMess.message);
                },
                function (error) {
                    columnChartViewManager.setChartStatus(ColumnChartViewManager.DISPLAY_SETTING_STATUS);
                }
            );
        }

        initDefaultSettingValueFromUserData();

        SCRIPT_PARENT.appendChild(columnChartViewManager.chartContainer);
        columnChartViewManager.columnChartOptions.width = columnChartViewManager.mainChart.clientWidth;
        columnChartViewManager.columnChartOptions.height = columnChartViewManager.mainChart.clientHeight;

        columnChartViewManager
            .columnChartSettingViewManager
            .addChooserPanelSelectionChangedCallback(function (e, m) {

            });

        columnChartViewManager
            .columnChartSettingViewManager
            .reloadButton.addEventListener("click", function (e) {
                if (e.isSuccess) {
                    reloadBarChart();
                }
            })

        columnChartViewManager
            .columnChartSettingViewManager
            .saveButton.addEventListener("click", async function (e) {
                if (e.isSuccess) {
                    columnChartViewManager.setChartStatus(ColumnChartViewManager.LOADING_STATUS);
                    await new Promise(r => setTimeout(r, 2000));
                    userDataManager.userData = new UserData(columnChartViewManager.dataRetrievalOption.param);
                    userDataManager.exportUserData();
                    columnChartViewManager.setChartStatus(ColumnChartViewManager.DISPLAY_SETTING_STATUS);
                }
            })

        columnChartViewManager
            .settingButton.addEventListener("click", function () {
                columnChartViewManager.setChartStatus(ColumnChartViewManager.DISPLAY_SETTING_STATUS);
            });

        initChartContentFromUserData();
    }

    //#region Controller
    class ColumnChartViewManager {
        static LOADING_STATUS = 1;
        static DISPLAY_CHART_STATUS = 2;
        static DISPLAY_SETTING_STATUS = 3;
        static DATA_RETRIEVAL_DEFAULT_OPTION = 2;
        static DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION = 1;

        static GET_DATA_SUCESSFULLY_MESSAGE = 1;
        static GET_DATA_FAIL_MESSAGE = 2;

        constructor() {
            const VM_INSTANCE = this;
            VM_INSTANCE.columnChartSettingViewManager = new ColumnChartSettingViewManager(false);

            VM_INSTANCE.chartLoaderContainer = document.createElement('div');
            VM_INSTANCE.chartLoaderContainer.className = 'huy_td1-loader-container';
            VM_INSTANCE.chartLoaderContainer.innerHTML = '<div class="huy_td1-loader1"></div>';

            VM_INSTANCE.mainChart = document.createElement('div');
            VM_INSTANCE.mainChart.className = 'huy_td1-column-chart-main-chart';

            VM_INSTANCE.mainChartContainer = document.createElement('div');
            VM_INSTANCE.mainChartContainer.className = 'huy_td1-column-chart-main-chart-container';
            VM_INSTANCE.mainChartContainer.appendChild(VM_INSTANCE.mainChart);

            VM_INSTANCE.settingButton = document.createElement('button');
            VM_INSTANCE.settingButton.className = "huy_td1-column-chart-setting-buton";
            VM_INSTANCE.settingButton.innerHTML = '<svg style="line-height: 0;font-family: Calibri;width: 1rem; height: 1rem;" viewBox="0 0 30 30"> <path d="m 13.340143,27.427522 c -0.735714,-0.184875 -1.285187,-0.65658 -1.563609,-1.34233 -0.07061,-0.17391 -0.09484,-0.46135 -0.119438,-1.41718 l -0.03087,-1.199025 -0.19673,-0.371375 c -0.332724,-0.628135 -0.946517,-1.05761 -1.6787741,-1.174655 -0.5786014,-0.09248 -0.8439264,-0.01845 -2.1802252,0.60829 L 6.3930291,23.083502 H 5.8435443 c -0.5063194,0 -0.5764578,-0.01195 -0.8928749,-0.15184 -0.5408201,-0.239145 -0.785574,-0.509815 -1.5343192,-1.69681 -0.3657186,-0.579775 -0.7135399,-1.156665 -0.7729405,-1.281975 -0.3399823,-0.7172 -0.056607,-1.682794 0.6505285,-2.216689 0.1600869,-0.120865 0.6377074,-0.406545 1.0613923,-0.63484 0.928602,-0.50038 1.2207719,-0.732455 1.4362182,-1.140835 0.3260925,-0.61811 0.3260925,-1.29994 0,-1.91805 -0.2166893,-0.41073 -0.5105685,-0.6429 -1.4441,-1.140835 C 3.4310766,12.412843 3.0638352,12.107229 2.8200522,11.630544 2.542276,11.087399 2.4984372,10.528704 2.6879542,9.9471739 2.7947338,9.619534 3.9800716,7.7246742 4.2646002,7.4267643 4.5999059,7.0757093 5.1464421,6.8366294 5.6795043,6.8078344 c 0.5629766,-0.0304 0.8311526,0.051935 2.0499684,0.6294748 1.1750713,0.556805 1.4449458,0.627955 2.0246667,0.53375 0.7448596,-0.12104 1.3804336,-0.5756199 1.7092586,-1.2225048 l 0.16135,-0.31742 0.03182,-1.1475999 c 0.03376,-1.2164647 0.05745,-1.3433247 0.33327,-1.7825197 C 12.147469,3.2500299 12.566837,2.88185 12.847009,2.74847 13.299935,2.53285 13.584785,2.5 15.00165,2.5 c 1.416875,0 1.701725,0.03285 2.154651,0.24847 0.280172,0.13338 0.699535,0.5015599 0.857165,0.7525448 0.278818,0.443965 0.299956,0.560525 0.332737,1.8339497 l 0.03087,1.1990199 0.194983,0.3680849 c 0.30399,0.5738749 0.833178,0.9814649 1.477551,1.1380449 0.684934,0.166435 1.09277,0.07166 2.147701,-0.4991 1.064999,-0.5761899 1.310607,-0.6865349 1.660591,-0.7460648 0.617447,-0.10502 1.321564,0.1240849 1.818458,0.5916899 0.329414,0.3099849 1.516156,2.1474647 1.713524,2.6530997 0.219203,0.5616 0.106734,1.222019 -0.297935,1.749354 -0.250789,0.326815 -0.464124,0.475605 -1.488155,1.037955 -0.871285,0.47847 -1.153714,0.714845 -1.390581,1.163835 -0.329426,0.624435 -0.331457,1.29526 -0.0057,1.912565 0.237178,0.449585 0.570306,0.725555 1.446012,1.1979 0.865531,0.46686 1.140356,0.67034 1.384131,1.024845 0.38014,0.552795 0.493862,1.391709 0.275356,2.031319 -0.126908,0.37148 -1.269408,2.19798 -1.574218,2.516685 -0.150441,0.1573 -0.358021,0.29749 -0.618122,0.417445 -0.332362,0.15328 -0.457163,0.183815 -0.824911,0.201825 -0.602664,0.0295 -0.845105,-0.0494 -1.862866,-0.60652 -0.474915,-0.25996 -0.969447,-0.511095 -1.09897,-0.55807 -1.030926,-0.37392 -2.24575,0.09547 -2.760032,1.066435 l -0.196735,0.371415 -0.03108,1.150155 c -0.03296,1.21963 -0.05638,1.345355 -0.332523,1.785075 -0.157631,0.25099 -0.576994,0.61917 -0.857166,0.752545 -0.456741,0.21744 -0.734149,0.248665 -2.178725,0.24526 -1.000513,-0.0025 -1.435735,-0.02155 -1.637427,-0.07224 z m 3.021043,-8.465505 c 1.404337,-0.444804 2.416419,-1.386239 2.879996,-2.678959 0.143465,-0.400065 0.174735,-0.56986 0.196414,-1.066595 0.01517,-0.347675 5.5e-4,-0.72558 -0.03531,-0.918335 -0.261267,-1.400015 -1.355184,-2.656549 -2.75129,-3.160289 -0.667466,-0.24083 -1.058757,-0.299455 -1.832486,-0.274545 -0.592414,0.01905 -0.746194,0.04255 -1.190214,0.181585 -0.961013,0.300965 -1.777345,0.88126 -2.335918,1.660514 -0.503292,0.702135 -0.738927,1.434335 -0.738927,2.296095 0,0.86165 0.236219,1.59615 0.736817,2.291055 0.521274,0.723605 1.251457,1.28116 2.058488,1.57182 0.676756,0.243744 0.968091,0.287914 1.783747,0.270459 0.662671,-0.0142 0.780031,-0.0307 1.228707,-0.172805 z"> </path> </svg>';

            VM_INSTANCE.chartContainer = document.createElement('div');
            VM_INSTANCE.chartContainer.className = 'huy_td1-column-chart-container';
            VM_INSTANCE.chartContainer.appendChild(VM_INSTANCE.columnChartSettingViewManager.settingContainer);
            VM_INSTANCE.chartContainer.appendChild(VM_INSTANCE.chartLoaderContainer);
            VM_INSTANCE.chartContainer.appendChild(VM_INSTANCE.mainChartContainer);
            VM_INSTANCE.chartContainer.appendChild(VM_INSTANCE.settingButton);
            VM_INSTANCE.setChartStatus(ColumnChartViewManager.DISPLAY_SETTING_STATUS);

            VM_INSTANCE.columnChartOptions = {
                fontName: 'Calibri',
                width: 0,
                height: 0,
                backgroundColor: 'blue',
                legend: { position: "none" },
                animation: {
                    startup: true,
                    duration: 2000,
                    easing: 'out',
                },
                chartArea: {
                    height: '80%',
                    width: '80%',
                    top: '50',
                    left: '80',
                    bottom: '80',
                    right: '100',
                },
                bar: {
                    gap: "50%",
                    groupWidth: 50,
                },
                vAxis: {
                    textStyle: {
                        bold: true,
                        fontSize: 15,
                    },
                    titleTextStyle: {
                        color: 'black',
                        bold: true,
                        italic: false,
                        fontSize: 15,
                    },
                },
                hAxis: {
                    textStyle: {
                        bold: true,
                        fontSize: 12,
                    },
                    titleTextStyle: {
                        color: 'black',
                        bold: true,
                        italic: false,
                        fontSize: 20,
                    },
                },
                bars: 'vertical',
            };

            VM_INSTANCE.dataRetrievalOption = new DataRetrievalOption();

            VM_INSTANCE.columnChartSettingViewManager.addChooserPanelSelectionChangedCallback(function (ele, index) {
                VM_INSTANCE.dataRetrievalOption.option = index;
            });

            VM_INSTANCE.columnChartSettingViewManager
                .saveButton.addEventListener("click", beforeSaveButtonClick);
            VM_INSTANCE.columnChartSettingViewManager
                .reloadButton.addEventListener("click", beforeSaveButtonClick);

            function beforeSaveButtonClick(e) {
                e.isSuccess = false;
                if (VM_INSTANCE.dataRetrievalOption.option == ColumnChartViewManager.DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION) {
                    if (!VM_INSTANCE.columnChartSettingViewManager.uiExpandButtonIDMainInput.value) {
                        alert('Please enter UI expand button label!');
                        return;
                    }
                    if (!VM_INSTANCE.columnChartSettingViewManager.tableColumnIDMainInput.value) {
                        alert('Please enter table column id!');
                        return;
                    }
                    if (VM_INSTANCE.columnChartSettingViewManager.filterItemsList.length == 0) {
                        alert('Please enter at least 1 item!');
                        return;
                    }
                    e.isSuccess = true;
                    VM_INSTANCE.dataRetrievalOption.param = {
                        uiExpandBtnID: VM_INSTANCE.columnChartSettingViewManager.uiExpandButtonIDMainInput.value,
                        tableColumnID: VM_INSTANCE.columnChartSettingViewManager.tableColumnIDMainInput.value,
                        filterItems: VM_INSTANCE.columnChartSettingViewManager.filterItemsList,
                        xAxisName: VM_INSTANCE.columnChartSettingViewManager.xAxisNameMainInput.value,
                        yAxisName: VM_INSTANCE.columnChartSettingViewManager.yAxisNameMainInput.value,
                    }
                }
                else if (VM_INSTANCE.dataRetrievalOption.option == ColumnChartViewManager.DATA_RETRIEVAL_DEFAULT_OPTION) {
                    e.isSuccess = true;
                    VM_INSTANCE.dataRetrievalOption.param = {
                        uiExpandBtnID: "",
                        tableColumnID: "",
                        filterItems: [],
                        xAxisName: VM_INSTANCE.columnChartSettingViewManager.xAxisNameMainInput.value,
                        yAxisName: VM_INSTANCE.columnChartSettingViewManager.yAxisNameMainInput.value,
                    }
                }
            }
        }

        setChartStatus(chartStatus) {
            switch (chartStatus) {
                case ColumnChartViewManager.LOADING_STATUS: {
                    this.chartLoaderContainer.style.display = 'flex';
                    this.mainChartContainer.style.display = 'none';
                    this.columnChartSettingViewManager.settingContainer.style.display = 'none';
                    this.settingButton.style.display = 'none';
                    break;
                }
                case ColumnChartViewManager.DISPLAY_CHART_STATUS: {
                    this.chartLoaderContainer.style.display = 'none';
                    this.mainChartContainer.style.display = 'grid';
                    this.columnChartSettingViewManager.settingContainer.style.display = 'none';
                    this.settingButton.style.display = 'inline-block';
                    break;
                }
                case ColumnChartViewManager.DISPLAY_SETTING_STATUS: {
                    this.chartLoaderContainer.style.display = 'none';
                    this.mainChartContainer.style.display = 'none';
                    this.columnChartSettingViewManager.settingContainer.style.display = 'grid';
                    this.settingButton.style.display = 'inline-block';
                    break;
                }
            }
        }

        /**
         * @param {ChartData} content
         * @param {number} message
         **/
        updateChartViewByContent(content, message) {

            /** 
            * @param {ChartData} rawData
            **/
            function createDataTable(rawData) {
                let arrayData = [];
                arrayData[0] = [rawData.xAxisName, rawData.yAxisName, { role: 'style' }];
                for (let i = 0; i < rawData.arrayData.length; i++) {
                    arrayData[i + 1] = [rawData.arrayData[i].key, rawData.arrayData[i].value, "#3B5A82"];
                }
                return google.visualization.arrayToDataTable(arrayData);
            }

            switch (message) {
                case ColumnChartViewManager.GET_DATA_SUCESSFULLY_MESSAGE: {
                    let tableData = createDataTable(content);
                    let chartOptions = this.columnChartOptions;
                    chartOptions.hAxis.title = content.xAxisName;
                    chartOptions.vAxis.title = content.yAxisName;

                    if (tableData != null) {
                        this.setChartStatus(ColumnChartViewManager.DISPLAY_CHART_STATUS);
                        chartOptions.width = this.mainChart.clientWidth;
                        chartOptions.height = this.mainChart.clientHeight;
                        let chart = new google.visualization.ColumnChart(this.mainChart);
                        chart.draw(tableData, chartOptions);
                    } else {
                        this.setChartStatus(ColumnChartViewManager.DISPLAY_SETTING_STATUS);
                    }
                    break;
                }
                case ColumnChartViewManager.GET_DATA_FAIL_MESSAGE: {
                    this.setChartStatus(ColumnChartViewManager.DISPLAY_SETTING_STATUS);
                    break;
                }
            }
        }

        /**
        * @param {UserData} userData
        **/
        setDataRetrievalOptionFromUserData(userData) {
            if (userData.uiExpandBtnID && !IsEmptyOrSpaces(userData.uiExpandBtnID)) {
                this.dataRetrievalOption.option = ColumnChartViewManager.DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION;
            } else {
                this.dataRetrievalOption.option = ColumnChartViewManager.DATA_RETRIEVAL_DEFAULT_OPTION;
            }

            this.dataRetrievalOption.param = {
                uiExpandBtnID: userData.uiExpandBtnID,
                tableColumnID: userData.tableColumnID,
                filterItems: userData.filterItems,
                xAxisName: userData.xAxisName,
                yAxisName: userData.yAxisName,
            }

            this.columnChartSettingViewManager.setDataRetrievalOption(this.dataRetrievalOption.option);
        }
    }

    class ColumnChartSettingViewManager {

        /**
         * @param {Boolean} isEnableDuplicateItem
         * @param {UserData} userData
         **/
        constructor(isEnableDuplicateItem, userData) {
            const VM_INSTANCE = this;
            VM_INSTANCE.isEnableDuplicateItem = isEnableDuplicateItem ?? true;
            VM_INSTANCE.filterItemsList = userData?.filterItems ?? [];

            VM_INSTANCE.settingContainer = document.createElement('div');
            VM_INSTANCE.settingContainer.className = 'huy_td1-column-chart-setting-container';
            VM_INSTANCE.settingContainer.innerHTML = '<div class="huy_td1-column-chart-setting-container-left-field" style="grid-column: 2; display: grid; overflow: auto; padding-right: 10px; height: fit-content;"> <div class="huy_td1-column-chart-setting-container-left-field-data-retrieval-1st-option-container" style="display: grid;"> <label style="margin: 10px 0px 10px 0px; font-weight: 700;">UI Expand button<span style="color: rgb(219, 65, 65);">*</span></label> <div class="huy_td1-input-field-container"> <input class="huy_td1-input-maininput" type="text" required="true"> <div class="huy_td1-input-placeholder">Fill in the name of UI expand button contaning a table </div> </div> <label style="margin: 20px 0px 10px 0px; font-weight: 700;">Column<span style="color: rgb(219, 65, 65);">*</span></label> <div class="huy_td1-input-field-container"> <input class="huy_td1-input-maininput" type="text" required="true"> <div class="huy_td1-input-placeholder">Fill in the name of a tables\'s column to get statistics </div> </div> <label style=" margin: 20px 0px 10px 0px; font-weight: 700;">Filter items<span style="color: rgb(219, 65, 65);">*</span></label> <div class="huy_td1-multi-choices-input-container"> <ul class="huy_td1-multi-choices-input-listview"> <li class="huy_td1-multi-choices-input-search-field"> <input type="text" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false" class="huy_td1-multi-choices-input-maininput" style="width: 1ch;"> </li> </ul> </div> </div> <label style="margin: 20px 0px 10px 0px; font-weight: 700;">X Axis<span style="color: rgb(219, 65, 65);">*</span></label> <div class="huy_td1-input-field-container"> <input class="huy_td1-input-maininput" type="text" required="true"> <div class="huy_td1-input-placeholder">Fill in the name of X Axis</div> </div> <label style="margin: 20px 0px 10px 0px; font-weight: 700;">Y Axis<span style="color: rgb(219, 65, 65);">*</span></label> <div class="huy_td1-input-field-container"> <input class="huy_td1-input-maininput" type="text" required="true"> <div class="huy_td1-input-placeholder">Fill in the name of Y Axis</div> </div> </div> <div class="huy_td1-column-chart-setting-container-right-field" style="grid-column: 4; grid-row:1; margin-top: 41px;"> <form class="huy_td1-selection-chooser-panel"> <div class="huy_td1-selection-chooser-option-container"> <input class="option-one" name="radio" checked="true" type="radio"> <div> <label> <span></span>Data from table in UI expand </label> </div> </div> <div class="huy_td1-selection-chooser-option-container"> <input class="option-two" name="radio" type="radio"> <div> <label> <span></span>Default</label> </div> </div> </form> <div style="display: flex; justify-content: center; align-items: center;"> <button style="margin: 5px;" class="huy_td1-button">Save config</button> <button style="margin: 5px;" class="huy_td1-button">Reload</button> </div> </div>';

            VM_INSTANCE.settingContainerLeftField = VM_INSTANCE.settingContainer.getElementsByClassName('huy_td1-column-chart-setting-container-left-field')[0];
            VM_INSTANCE.settingContainerRightField = VM_INSTANCE.settingContainer.getElementsByClassName('huy_td1-column-chart-setting-container-right-field')[0];
            VM_INSTANCE.ostOptionSettingContainerLeftField = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-column-chart-setting-container-left-field-data-retrieval-1st-option-container")[0];

            VM_INSTANCE.multiChoicesMainInput = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-multi-choices-input-maininput")[0];
            VM_INSTANCE.saveButton = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-button")[0];
            VM_INSTANCE.reloadButton = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-button")[1];
            VM_INSTANCE.multiChoicesList = VM_INSTANCE.settingContainer.getElementsByClassName('huy_td1-multi-choices-input-listview')[0];
            VM_INSTANCE.multiChoiceContainer = VM_INSTANCE.settingContainer.getElementsByClassName('huy_td1-multi-choices-input-container')[0];

            VM_INSTANCE.uiExpandButtonIDMainInput = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-input-maininput")[0];
            VM_INSTANCE.tableColumnIDMainInput = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-input-maininput")[1];
            VM_INSTANCE.xAxisNameMainInput = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-input-maininput")[2];
            VM_INSTANCE.yAxisNameMainInput = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-input-maininput")[3];

            VM_INSTANCE.optionChooserPanel = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-selection-chooser-panel")[0];

            function initEventOfMultiChoices() {
                VM_INSTANCE.multiChoicesMainInput.addEventListener('input', resizeInput); // bind the "resizeInput" callback on "input" event
                resizeInput.call(VM_INSTANCE.multiChoicesMainInput); // immediately call the function
                VM_INSTANCE.multiChoicesMainInput.addEventListener('keydown', handleKeyDown)
                VM_INSTANCE.multiChoiceContainer.addEventListener('click', mainFocus);

                function mainFocus(e) {
                    if (e.target == this) {
                        VM_INSTANCE.multiChoicesMainInput.focus();
                    }
                }
                function resizeInput() {
                    this.style.width = (this.value.length + 1) + "ch";
                }

                function handleKeyDown(ele) {
                    if (ele.key == 'Enter') {
                        let isEmpty = this.value === null || this.value.match(/^ *$/) !== null;
                        if (!isEmpty) {
                            if (!VM_INSTANCE.isEnableDuplicateItem) {
                                if (!VM_INSTANCE.filterItemsList.includes(this.value)) {
                                    VM_INSTANCE.filterItemsList.push(this.value);
                                    inserNewElement(this.value);
                                    this.value = '';
                                    this.style.width = (this.value.length + 1) + "ch";
                                }
                            } else {
                                VM_INSTANCE.filterItemsList.push(this.value);
                                inserNewElement(this.value);
                                this.value = '';
                                this.style.width = (this.value.length + 1) + "ch";
                            }

                        }
                    }
                }

                function inserNewElement(newLabel) {
                    let li = document.createElement('li');
                    li.className = 'huy_td1-multi-choices-input-listview-item';
                    let divLabel = document.createElement('div');
                    divLabel.className = 'label-choice';
                    divLabel.innerText = newLabel;
                    let deleteBtn = document.createElement('button');
                    deleteBtn.className = 'btn-delete-choice';
                    deleteBtn.innerHTML = '<svg style="width: 6px; height: 6px;" viewBox="0 0 30 30"> <path d="m 5.73933,2.5 c -0.01995,0 -0.04,0.0077 -0.05528,0.023 L 2.522995,5.68404 c -0.03066,0.03064 -0.03066,0.07998 0,0.110596 l 9.205379,9.205378 -9.205379,9.20538 c -0.03066,0.03063 -0.03066,0.07997 0,0.110583 l 3.161055,3.161047 c 0.0306,0.03063 0.07995,0.03063 0.110583,0 l 9.205384,-9.20538 9.205385,9.20538 c 0.03064,0.03063 0.07971,0.03063 0.110323,0 l 3.161294,-3.161047 c 0.03064,-0.03063 0.03064,-0.07997 0,-0.110583 l -9.205369,-9.20538 9.205369,-9.205378 c 0.03064,-0.03064 0.03064,-0.07997 0,-0.110596 L 24.315729,2.523 c -0.03064,-0.03064 -0.07971,-0.03064 -0.110323,0 L 15.000015,11.728382 5.79463,2.523 C 5.77933,2.5077 5.75934,2.5 5.73933,2.5 Z"> </path> </svg>';
                    li.appendChild(divLabel);
                    li.appendChild(deleteBtn);
                    VM_INSTANCE.multiChoicesList.insertBefore(li, VM_INSTANCE.multiChoicesList.lastElementChild);

                    deleteBtn.addEventListener('click', deleteElement)

                    function deleteElement() {
                        VM_INSTANCE.multiChoicesList.removeChild(li);
                        let itemIndex = VM_INSTANCE.filterItemsList.indexOf(newLabel);
                        VM_INSTANCE.filterItemsList.splice(itemIndex, 1);
                    }
                }

            }
            initEventOfMultiChoices();
            VM_INSTANCE.chooserOptionItems = VM_INSTANCE.settingContainer.getElementsByClassName("huy_td1-selection-chooser-option-container");

            for (let i = 0; i < VM_INSTANCE.chooserOptionItems.length; i++) {
                VM_INSTANCE.chooserOptionItems[i].addEventListener("click", onOptionContainerClick(VM_INSTANCE.chooserOptionItems[i], i + 1));
            }

            function onOptionContainerClick(ele, optionIndex) {
                return function () {
                    let chooserPanel = ele.parentElement;
                    if (!chooserPanel.selectionChangedCallback) {
                        chooserPanel.selectionChangedCallback = [];
                    }
                    let inputRadio = ele.getElementsByTagName("input")[0];
                    if (!inputRadio.checked) {
                        switch (optionIndex) {
                            case ColumnChartViewManager.DATA_RETRIEVAL_DEFAULT_OPTION:
                                {
                                    VM_INSTANCE.ostOptionSettingContainerLeftField.style.display = 'none';
                                    break;
                                }
                            case ColumnChartViewManager.DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION:
                                {
                                    VM_INSTANCE.ostOptionSettingContainerLeftField.style.display = 'grid';
                                    break;
                                }
                        }
                        inputRadio.checked = true;
                        for (var i = 0; i < chooserPanel?.selectionChangedCallback.length; i++) {
                            chooserPanel.selectionChangedCallback[i].call(ele, ele, optionIndex);
                        }
                    }
                }
            }
        }

        /**
        * @param {function} callback
        **/
        addChooserPanelSelectionChangedCallback(callback) {
            if (!this.optionChooserPanel.selectionChangedCallback) {
                this.optionChooserPanel.selectionChangedCallback = [];
            }
            let index = this.optionChooserPanel.selectionChangedCallback.length;
            this.optionChooserPanel.selectionChangedCallback[index] = callback;
        }

        /**
        * @param {String} newLabel
        **/
        insertElementForMultiChoices(newLabel) {
            const viewManager = this;
            let li = document.createElement('li');
            li.className = 'huy_td1-multi-choices-input-listview-item';
            let divLabel = document.createElement('div');
            divLabel.className = 'label-choice';
            divLabel.innerText = newLabel;
            let deleteBtn = document.createElement('button');
            deleteBtn.className = 'btn-delete-choice';
            deleteBtn.innerHTML = '<svg style="width: 6px; height: 6px;" viewBox="0 0 30 30"> <path d="m 5.73933,2.5 c -0.01995,0 -0.04,0.0077 -0.05528,0.023 L 2.522995,5.68404 c -0.03066,0.03064 -0.03066,0.07998 0,0.110596 l 9.205379,9.205378 -9.205379,9.20538 c -0.03066,0.03063 -0.03066,0.07997 0,0.110583 l 3.161055,3.161047 c 0.0306,0.03063 0.07995,0.03063 0.110583,0 l 9.205384,-9.20538 9.205385,9.20538 c 0.03064,0.03063 0.07971,0.03063 0.110323,0 l 3.161294,-3.161047 c 0.03064,-0.03063 0.03064,-0.07997 0,-0.110583 l -9.205369,-9.20538 9.205369,-9.205378 c 0.03064,-0.03064 0.03064,-0.07997 0,-0.110596 L 24.315729,2.523 c -0.03064,-0.03064 -0.07971,-0.03064 -0.110323,0 L 15.000015,11.728382 5.79463,2.523 C 5.77933,2.5077 5.75934,2.5 5.73933,2.5 Z"> </path> </svg>';
            li.appendChild(divLabel);
            li.appendChild(deleteBtn);
            this.multiChoicesList.insertBefore(li, this.multiChoicesList.lastElementChild);

            deleteBtn.addEventListener('click', deleteElement)

            function deleteElement() {
                viewManager.multiChoicesList.removeChild(li);
                let itemIndex = viewManager.filterItemsList.indexOf(newLabel);
                viewManager.filterItemsList.splice(itemIndex, 1);
            }
        }

        /**
        * @param {number} option
        **/
        setDataRetrievalOption(option) {
            switch (option) {
                case ColumnChartViewManager.DATA_RETRIEVAL_DEFAULT_OPTION:
                    {
                        this.ostOptionSettingContainerLeftField.style.display = 'none';
                        this.chooserOptionItems[1].getElementsByTagName("input")[0].checked = true;
                        break;
                    }
                case ColumnChartViewManager.DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION:
                    {
                        this.ostOptionSettingContainerLeftField.style.display = 'grid';
                        this.chooserOptionItems[0].getElementsByTagName("input")[0].checked = true;
                        break;
                    }
            }
        }
    }

    class UserDataManager {
        constructor(scriptIndex) {
            this.LOCAL_STORAGE_USER_DATA_KEY = window.location.origin + window.location.pathname + 'UserDataManager';
            this.scriptIndex = scriptIndex;
            this.userData = new UserData();
            this.localStorageData = null;

            this.init();
        }

        init() {
            var localStorageDataJSON = localStorage[this.LOCAL_STORAGE_USER_DATA_KEY];
            var localStorageData = null;
            if (localStorageDataJSON) {
                localStorageData = JSON.parse(localStorageDataJSON);
            }
            if (localStorageData) {
                let data = localStorageData[this.scriptIndex + ''];
                if (data == null) {
                    data = new UserData();
                } else {
                    data = new UserData(data);
                }
                localStorageData[this.scriptIndex + ''] = data;
                this.userData = data;
                this.localStorageData = localStorageData;
            } else {
                var localStorageData = {};
                localStorageData[this.scriptIndex + ''] = new UserData();
                localStorage.setItem(this.LOCAL_STORAGE_USER_DATA_KEY, JSON.stringify(localStorageData));
                this.userData = localStorageData[this.scriptIndex + ''];
                this.localStorageData = localStorageData;
            }
        }

        exportUserData() {
            var localStorageDataJSON = localStorage[this.LOCAL_STORAGE_USER_DATA_KEY];
            var localStorageData = null;
            if (localStorageDataJSON) {
                this.localStorageData = JSON.parse(localStorageDataJSON);
            }
            this.localStorageData[this.scriptIndex + ''] = this.userData;
            localStorage.setItem(this.LOCAL_STORAGE_USER_DATA_KEY, JSON.stringify(this.localStorageData));
        }
    }
    //#endregion

    //#region Model
    class UserData {
        constructor(obj) {
            if (obj) {
                Object.assign(this, obj);
            } else {
                this.filterItems = [];
                this.tableColumnID = '';
                this.uiExpandBtnID = '';
                this.xAxisName = '';
                this.yAxisName = '';
            }
        }
    }

    class ChartData {
        constructor(xAxisName, yAxisName) {
            this.arrayData = [];
            this.xAxisName = xAxisName;
            this.yAxisName = yAxisName;
        }

        addItemToChartData(key, value) {
            class ChartDataItem {
                constructor(k, v) {
                    this.key = k;
                    this.value = v;
                }
            }
            this.arrayData[this.arrayData.length] = new ChartDataItem(key, value);
        }
    }

    class DataRetrievalOption {
        constructor() {
            this.option = ColumnChartViewManager.DATA_RETRIEVAL_FROM_UI_EXPAND_OPTION;
            this.param = '';
        }
    }
    //#endregion

    onChartCreate();
}
