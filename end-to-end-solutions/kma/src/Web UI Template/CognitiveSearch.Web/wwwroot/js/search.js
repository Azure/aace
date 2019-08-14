// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Initialize properties
var q, sortType, tempdata, instrumentationKey;
var results = [];
var facets = [];
var token = "";
var selectedFacets = [];
var currentPage = 1;
var searchId;
var searchServiceName = "";
var indexName = "";
var scoringProfile = "";

// When 'Enter' clicked from Search Box, execute Search()
$("#q").keyup(function (e) {
    if (e.keyCode === 13) {
        Search();
    }
});

$("#transcript-search-input").keyup(function (e) {
    if (e.keyCode === 13) {
        SearchTranscript($('#transcript-search-input').val());
    }
});

function getOffset(ele) {
    var _x = 0;
    var _y = 0;
    var el = ele;
    while (el && !isNaN(el.offsetLeft) && !isNaN(el.offsetTop)) {
        _x += el.offsetLeft - el.scrollLeft;
        _y += el.offsetTop - el.scrollTop;
        el = el.offsetParent;
    }
    _y = _y + ele.offsetHeight + 10;
    return { top: _y, left: _x };
}


var tutorialList = [
    {
        text: "Start from here. Enter your search query and click the search button.",
        refNodeId: "btn-search",
        modalClass: "left-tutorial-modal-content",
        offset_x: 0,
        offset_y: 0
    },
    {
        text: "The search results will be shown on the right side. Click on the image or title of the document to view the details.",
        refNodeId: "",
        modalClass: "left-tutorial-modal-content",
        offset_x: 400,
        offset_y: 260
    },
    {
        text: "You can filter the search result by check the pre-extracted entities.",
        refNodeId: "keyPhrases-facets",
        modalClass: "left-tutorial-modal-content",
        offset_x: 0,
        offset_y: 0
    },
    {
        text: "You can configure the facets here. To remove words or phrases from certain type of entities, add them to the restriction list and click 'update and re-index'. (only supported in self-deployed demo environment)",
        refNodeId: "facet-settings-btn",
        modalClass: "left-tutorial-modal-content",
        offset_x: 0,
        offset_y: 0
    },
    {
        text: "Customize headline or image on the homepage, or use your own logo. (only supported in self-deployed demo environment)",
        refNodeId: "customize-page",
        modalClass: "left-tutorial-modal-content",
        offset_x: 0,
        offset_y: 12
    },
    {
        text: "Upload and index your own files. (only supported in self-deployed demo environment)",
        refNodeId: "upload-data-page",
        modalClass: "left-tutorial-modal-content",
        offset_x: 0,
        offset_y: 12
    },
    {
        text: "Switch to the graph view to find out relationship between the search keyword and pre-extracted entities.",
        refNodeId: "view-graph-button",
        modalClass: "right-tutorial-modal-content",
        offset_x: 0,
        offset_y: 0
    }
];

function showTutorialSection(i) {
    var section = tutorialList[i];
    hideAllTutorialModals();
    var modal = document.getElementById("tutorial-modal");
    modal.style.display = "block";
    var content = document.getElementById("tutorial-modal-content");
    content.className = section.modalClass;
    var x = section.offset_x;
    var y = section.offset_y;
    if (section.refNodeId !== "") {
        var offset = getOffset(document.getElementById(section.refNodeId));
        x += offset.left;
        y += offset.top;
    }
    if (section.modalClass === "right-tutorial-modal-content") {
        x = x - 360;
    }

    content.style.left = x + "px";
    content.style.top = y + "px";
    content.childNodes[1].innerText = section.text;
    var linkR = document.getElementById("previous-tut-link");
    
    if (i === 0) {
        linkR.innerText = "";
    } else {
        linkR.innerText = "Previous";
        linkR.href = "javascript:showTutorialSection(" + (i - 1) + ")";
    }

    linkR = document.getElementById("next-tut-link");
    var skipLink = document.getElementById("skip-tut-link");
    if (i + 1 === tutorialList.length) {
        linkR.innerText = "";
        skipLink.innerText = "Try it now.";
    } else {
        linkR.innerText = "Next";
        skipLink.innerText = "Skip Tutorial.";
        linkR.href = "javascript:showTutorialSection(" + (i + 1) + ")";
    }

}

function hideAllTutorialModals() {
    var introModel = document.getElementById("introModal");
    introModel.style.display = "none";
    var modal = document.getElementById("tutorial-modal");
    modal.style.display = "none";
}

function skipTutorial(skip) {
    if (skip && $("#donotshowtutorial")[0].checked) {
        var now = new Date();
        var time = now.getTime();
        var expireTime = time + 1000 * 36000 * 365;
        now.setTime(expireTime);
        document.cookie = 'showTutorial=false;expires=' + now.toGMTString() + '; path=/ ';
    }
    hideAllTutorialModals();
}

function ShowFacetSettings() {
    
    var node = document.getElementById("facet-settings-modal");
    if (customizable === 'False') {
        node.innerHTML = `<h4>The functionality is not available in this deployment</h4>
            <p>
            This is a shared deployment.Some functionalities including customization and uploading files are not available in this deployment.
            </p >
            <p>
                If you want to use this functionality, you can create it your own deployment. You can find the deployment script and instruction in
                <a href="https://github.com/Azure/AIPlatform/tree/master/end-to-end-solutions/kma/Deployment" target="_blank">the GitHub repo.</a>
            </p>`;
    }
    if (node.style.display === 'none') {
        //GetFacetFilterFiles();
        $("#facet-settings-btn").text("Facet Settings <<");
        node.style.display = 'block';
    } else {
        node.style.display = 'none';
        $("#facet-settings-btn").text("Facet Settings >>");
    }
}

function GetFacetFilterFiles() {
    //$("#facet-settings-modal").html('');

    // Get center of map to use to score the search results
    $.ajax({
        url: `${apiUrl}/facets/`,
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            var selectNode = $("#facet-settings-select")[0];
            for (var i = selectNode.options.length - 1; i >= 0; i--) {
                selectNode.remove(i);
            }
            for (i = 0; i < data.length; i++) {
                var opt = document.createElement('option');
                // create text node to add to option element (opt)
                opt.appendChild(document.createTextNode(data[i].name));

                // set value property of opt
                opt.value = data[i].restrictionList;
                selectNode.appendChild(opt);
            }
            $("#facet-settings-select")[0].selectedIndex = 0;
            $("#facet-settings-text").val($("#facet-settings-select").val());

            $("#facet_settings-checkbox")[0].checked = IsFacetHidden(data[0].name);
        }

    });
}

function IsFacetHidden(facetName) {
    var hiddenFacets = getCookie("hiddenFacets").split(',')
    for (var i = 0; i < hiddenFacets.length; i++)
    {
        if (hiddenFacets[i] === facetName) {
            return true;
        }
    }
    return false;
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function ResetAndRunIndexer() {
    $.ajax({
        url: `${apiUrl}/indexer/`,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({
            "reset": 'true',
            "run": 'true'
        }),
        dataType: 'json',
        success: function (data) {
            $("#facet-settings-status").text("Index reset successfully!");
        }
    });
}

function SetHiddenFacetCookie(facetName, hide) {
    var cookie = getCookie("hiddenFacets");
    if (hide) {
        document.cookie = "hiddenFacets=" + cookie + facetName + ",;"
    } else {
        document.cookie = "hiddenFacets=" + cookie.replace(facetName + ",", "") + ";";
    }
}

function OnFacetSettingSelectChange() {
    $("#facet-settings-text").val($("#facet-settings-select").val());

    if (IsFacetHidden($("#facet-settings-select>option:selected").text())) {
        $("#facet_settings-checkbox")[0].checked = true;
    } else {
        $("#facet_settings-checkbox")[0].checked = false;
    }
}

function SaveFacetSettingsAndResetIndexer() {
    var res = window.confirm("Re-indexing may take a long time. During the progress, partial search result may shown. Do you want to continue?");
    if (res) {
        SaveFacetSettings();
        ResetAndRunIndexer();
        $("#facet-settings-status").text("Setting saved! Indexer reset started!");
    }
}

function SaveFacetSettings() {
    var name = $("#facet-settings-select>option:selected").text();
    if (IsFacetHidden(name) !== $("#facet_settings-checkbox")[0].checked) {
        SetHiddenFacetCookie(name, $("#facet_settings-checkbox")[0].checked);
        UpdateFacets();
    }

    $.ajax({
        url: `${apiUrl}/facets/`,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({
            "facetName": $("#facet-settings-select>option:selected").text(),
            "text": $("#facet-settings-text").val()
        }),
        dataType: 'json',
        success: function (data) {
            $("#facet-settings-status").text("Setting saved!");
        }
    });
    $("#facet-settings-select>option:selected").val($("#facet-settings-text").val());
}

// Search with query and facets
function Search() {
    if (results && results.length > 0) {
        $('#loading-indicator').show();
    }
    else $('#progress-indicator').show();

    if (currentPage > 1) {
        if (q !== $("#q").val()) {
            currentPage = 1;
        }
    }
    q = $("#q").val();

    //var url = 'https://localhost:44311/api';

    // Get center of map to use to score the search results
    $.ajax({
        url: `${apiUrl}/documents/`,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({
            "query": q,
            "searchFacets": selectedFacets,
            "currentPage": currentPage
        }),
        dataType: 'json',
        success: function (data) {
            if (document.getElementById("results-entity-map").style.display === "none") {
                $('#loading-indicator').css("display", "none");
                $('#progress-indicator').css("display", "none");
                Update(data, q);
            } else {
                LoadEntityMap();
            }
        }
    });
}

function Update(data, q) {
    results = data.results;
    facets = data.facets;
    tags = data.tags;
    token = data.token;
    searchId = data.searchId;

    //Facets
    UpdateFacets();

    //Results List
    UpdateResults(data, q);

    //Pagination
    UpdatePagination(data.count);

    // Log Search Events
    LogSearchAnalytics(data.count);

    //Filters
    UpdateFilterReset();

    InitLayout();

    $('html, body').animate({ scrollTop: 0 }, 'fast');

    FabricInit();
}

function UpdatePagination(docCount) {
    var totalPages = Math.round(docCount / 10);
    // Set a max of 5 items and set the current page in middle of pages
    var startPage = currentPage;

    var maxPage = startPage + 5;
    if (totalPages < maxPage)
        maxPage = totalPages + 1;
    var backPage = parseInt(currentPage) - 1;
    if (backPage < 1)
        backPage = 1;
    var forwardPage = parseInt(currentPage) + 1;

    var htmlString = "Page: ";
    if (currentPage > 1) {
        htmlString += `<li><a href="javascript:void(0)" onclick="GoToPage('${backPage}')" class="ms-Icon ms-Icon--ChevronLeftMed"></a></li>`;
    }

    htmlString += '<li class="active"><a href="#">' + currentPage + '</a></li>';

    if (currentPage <= totalPages) {
        htmlString += `<li><a href="javascript:void(0)" onclick="GoToPage('${forwardPage}')" class="ms-Icon ms-Icon--ChevronRightMed"></a></li>`;
    }
    var firstDoc = (currentPage - 1) * 10 + 1;
    var lastDoc = (currentPage) * 10;
    if (lastDoc > docCount) {
        lastDoc = docCount;
    }
    if (firstDoc > docCount) {
        firstDoc = docCount;
    }
    htmlString += '&nbsp;&nbsp;[showing documents ' + firstDoc + ' to ' + lastDoc+']';
    $("#pagination").html(htmlString);
    $("#paginationFooter").html(htmlString);
}

function GoToPage(page) {
    currentPage = page;
    Search();
}

function SampleSearch(text) {
    $('#index-search-input').val(text);
    $('#index-search-submit').click();
}