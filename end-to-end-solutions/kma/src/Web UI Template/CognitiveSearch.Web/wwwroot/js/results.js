// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


function ShowHideTags(i) {
    var node = document.getElementById("tagdiv" + i);
    if (node.style.display === "none") {
        node.style.display = "block";
    }
    else {
        node.style.display = "none";
    }


    for (var j = 0; j < 10; j++) {
        var id = "resultdiv" + j;
        if (document.getElementById(id) === null) {
            break;
        }
        document.getElementById(id).style.top = 15 + 'px';
        document.getElementById(id).style.position = 'relative';
    }

}

function UpdateResults(data, q) {
    var resultsHtml = '';

    if (data.count !== 0) {
        startDocCount = 1;
    }
    var currentDocCount = currentPage * 10;

    if (currentPage > 1) {
        startDocCount = ((currentPage - 1) * 10) + 1;
    }
    if (currentDocCount > data.count) {
        currentDocCount = data.count;
    }

    $("#doc-count").html(` Available Results: ${data.count}`);

    for (var i = 0; i < data.results.length; i++) {

        var result = data.results[i].document;
        result.idx = i;
        if (typeof result.tagDisplay === 'undefined') {
            result.tagDisplay = 'none';
        }

        var id = result.id;
        var name = result.metadata_storage_name.split(".")[0];
        var path = result.metadata_storage_path + token;
        var summary = generateSummary(result.content, q);
        var highlightedSummary = highlight(summary, q);
        var tags = GetTagsHTML(result);

        if (path !== null) {
            var classList = "results-div ";
            if (i === 0) classList += "results-sizer";

            var pathLower = path.toLowerCase();

            if (pathLower.includes(".jpg") || pathLower.includes(".png")) {
                resultsHtml += `<div id="resultdiv${i}" class="${classList}">
                                    <div class="search-result">
                                        <img class="img-result" style='max-width:100%;' src="${path}"  onclick="ShowDocument('${id}');"/>
                                        <div class="results-header">
                                            <h4>${name}
                                                <img src="/images/${buttonIcon}" height="30px" onclick="ShowHideTags(${i});">
                                            </h4>
                                            <div id="tagdiv${i}" class="tag-container" style="margin-top:10px;display:${result.tagDisplay}">${tags}</div>
                                        </div>
                                    </div>
                                </div>`;
            }
            else if (pathLower.includes(".mp3")) {
                resultsHtml += `<div id="resultdiv${i}" class="${classList}">
                                    <div class="search-result">
                                        <div class="audio-result-div" onclick="ShowDocument('${id}');">
                                            <audio controls>
                                                <source src="${path}" type="audio/mp3">
                                                Your browser does not support the audio tag.
                                            </audio>
                                        </div>
                                        <div class="results-header">
                                            <h4>${name}
                                                <img src="/images/${buttonIcon}" height="30px" onclick="ShowHideTags(${i});">
                                            </h4>
                                            <div id="tagdiv${i}" class="tag-container" style="margin-top:10px;display:${result.tagDisplay}">${tags}</div>
                                        </div>                               
                                    </div>
                                </div>`;
            }
            else if (pathLower.includes(".mp4")) {
                resultsHtml += `<div id="resultdiv${i}" class="${classList}">
                                    <div class="search-result">
                                        <div class="video-result-div" onclick="ShowDocument('${id}');">
                                            <video controls class="video-result">
                                                <source src="${path}" type="video/mp4">
                                                Your browser does not support the video tag.
                                            </video>
                                        </div>
                                        <hr />
                                        <div class="results-header">
                                            <h4>${name}
                                                <img src="/images/${buttonIcon}" height="30px" onclick="ShowHideTags(${i});">
                                            </h4>
                                            <div id="tagdiv${i}" class="tag-container" style="margin-top:10px;display:${result.tagDisplay}">${tags}</div>
                                        </div>  
                                    </div>
                                </div>`;
            }
            else {
                var icon = " ms-Icon--Page";

                if (pathLower.includes(".pdf")) {
                    icon = "ms-Icon--PDF";
                }
                else if (pathLower.includes(".htm")) {
                    icon = "ms-Icon--FileHTML";
                }
                else if (pathLower.includes(".xml")) {
                    icon = "ms-Icon--FileCode";
                }
                else if (pathLower.includes(".doc")) {
                    icon = "ms-Icon--WordDocument";
                }
                else if (pathLower.includes(".ppt")) {
                    icon = "ms-Icon--PowerPointDocument";
                }
                else if (pathLower.includes(".xls")) {
                    icon = "ms-Icon--ExcelDocument";
                }

                var buttonIcon = "expand.png";
                if (result.tagDisplay === "block") {
                    buttonIcon = "collapse.png"
                }

                resultsHtml += `<div id="resultdiv${i}" class="${classList}">
                                    <div class="search-result">
                                        <div class="card mt-3">
                                            <span class="card-header" onclick="ShowDocument('${id}');">
                                                <strong>
                                                    <i class="html-icon ms-Icon ${icon}"></i>${name}
                                                </strong>
                                                <small><a href="${path}">Dowload</a></small>
                                            </span>
                                            <div class="card-body">
                                                <p>${highlightedSummary}</p>
                                            </div>
                                            <div class="card-footer">
                                                <img src="/images/${buttonIcon}" height="30px" onclick="ShowHideTags(${i});">
                                                <div id="tagdiv${i}" class="tag-container" style="margin-top:10px;display:${result.tagDisplay}">${tags}</div>
                                            </div>
                                        </div>
                                    </div>
                                </div>`;
            }
        }
        else {
            // TODO: Handle errors showing result.
        }
    }

    $("#doc-details-div").html(resultsHtml);
}