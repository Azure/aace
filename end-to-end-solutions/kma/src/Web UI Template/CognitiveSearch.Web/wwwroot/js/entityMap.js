// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Graph Configuration
var nodeRadius = 15;
var nodeSeparationFactor = 1;
var nodeChargeStrength = -950;
var nodeChargeAccuracy = 0.4;
var nodeDistance = 100;

function SearchEntities(facet) {
    if (currentPage > 1) {
        if (q !== $("#q").val()) {
            currentPage = 1;
        }
    }
    q = $("#q").val();

    Unload();
    document.getElementById("entity-loading-indicator").style.display = "block";

    if (facet === null || facet === 'undefined') {
        facet = 'location';
    }

    GetGraph(q, facet);
    GetFacets(facet);
}

// Load Graph with Search data
function GetGraph(q, facet) {
    if (q === null) {
        q = "*";
    }

    if (facet === null) {
        facet = 'keyPhrases';
    }

    //var url = 'https://localhost:44311/api';

    $.ajax({
        type: "GET",
        url: `${apiUrl}/graph/${facet}?query=${q}`,
        success: function (data) {
            // Do something interesting here.
            update(data.links, data.nodes);
        }
    });
}

// Load Graph with Search data
function GetFacets(selectedFacet) {
    //var url = 'https://localhost:44311/api';

    if ($('#facet-picker')[0].options.length === 0) {
        $.ajax({
            type: "GET",
            url: `${apiUrl}/facets`,
            success: function (data) {
                var $select = $('#facet-picker');

                data.forEach(function (f) {
                    var opt = document.createElement('option');
                    opt.value = f.name;
                    opt.innerHTML = f.name;
                    if (f.name === selectedFacet) {
                        opt.selected = true;
                    }
                    $select.append(opt);
                });
            }
        });

    }
}

function LoadEntityMap() {
    document.getElementById("results-list-view").style.display = "none";
    document.getElementById("details-modal").style.display = "none";
    document.getElementById("results-entity-map").style.display = "block";
    document.getElementById("entity-loading-indicator").style.display = "block";
    Unload();
    GetFacets('location');
    GetGraph(q, 'locations');
    document.getElementById("q").value = q;
    q = q;
}

function UnloadEntityMap() {
    document.getElementById("results-entity-map").style.display = "none";
    document.getElementById("results-list-view").style.display = "block";
    Unload();

    document.getElementById("q").value = q;
    document.getElementById("btn-search").click();
}

function EntityMapClick(view) {
    if (view === 'entitymap') {
        LoadEntityMap();
    } else {
        UnloadEntityMap();
    }
}


function Unload() {
    svg.selectAll(".link").remove();
    svg.selectAll(".edgepath").remove();
    svg.selectAll(".node").remove();
    svg.selectAll(".edgelabel").remove();
    //var $select = $('#facet-picker');
    //$select.empty();
}

var colors = d3.scaleOrdinal(d3.schemeCategory10);
var svg = d3.select("svg"),
    width = +svg.attr("width"),
    height = +svg.attr("height"),
    node,
    link;

svg.append('defs').append('marker')
    .attrs({
        'id': 'arrowhead',
        'viewBox': '-0 -5 10 10',
        'refX': 25,
        'refY': 0,
        'orient': 'auto',
        'markerWidth': 10,
        'markerHeight': 10,
        'xoverflow': 'visible'
    })
    .append('svg:path')
    .attr('d', 'M 0,-5 L 10 ,0 L 0,5')
    .attr('fill', '#999')
    .style('stroke', 'none');

var simulation = d3.forceSimulation()
    .force("link", d3.forceLink()
        .id(function (d) { return d.id; })
        .distance(150).strength(.5))
    .force("charge", d3.forceManyBody()
        .strength(nodeChargeStrength)
        .theta(nodeChargeAccuracy))
    .force("center", d3.forceCenter(width / 2, height / 2))
    .force("collide", d3.forceCollide(nodeRadius));

function isDirectNeighbor(links, selectedNodeId, node) {
    for (var i = 0; i < links.length; i++) {
        if (links[i].source === selectedNodeId && links[i].target === node.id) {
            return true;
        }
    }
    return false;
}

function update(links, nodes, selectedNodeId) {

    if (typeof selectedNodeId === 'undefined' || selectedNode === null) {
        selectedNodeId = nodes[0].id;
    }

    for (var i = 0; i < nodes.length; i++) {
        nodes[i].isDirectNeighbor = isDirectNeighbor(links, selectedNodeId, nodes[i]);
        nodes[i].nodeRadius = nodeRadius;
        nodes[i].fontweight = "normal";
    }

    nodes[selectedNodeId].isDirectNeighbor = true;
    nodes[selectedNodeId].nodeRadius = nodeRadius * 2;
    nodes[selectedNodeId].fontweight = 'bold';

    // Graph implementation
    var colors = d3.scaleOrdinal(d3.schemeCategory10);
    var svg = d3.select("svg"),
        width = +svg.attr("width"),
        height = +svg.attr("height");

    svg.append('defs').append('marker')
        .attrs({
            'id': 'arrowhead',
            'viewBox': '-0 -5 10 10',
            'refX': 13,
            'refY': 0,
            'orient': 'auto',
            'markerWidth': 10,
            'markerHeight': 10,
            'xoverflow': 'visible'
        })
        .append('svg:path')
        .attr('d', 'M 0,-5 L 10 ,0 L 0,5')
        .attr('fill', '#999')
        .style('stroke', 'none');

    simulation = d3.forceSimulation()
        .force("link", d3.forceLink()
            .id(function (d) { return d.id; })
            .distance(150).strength(.5))
        .force("charge", d3.forceManyBody()
            .strength(nodeChargeStrength)
            .theta(nodeChargeAccuracy))
        .force("center", d3.forceCenter(width / 2, height / 2))
        .force("collide", d3.forceCollide(nodeRadius));


    link = svg.selectAll(".link")
        .data(links)
        .enter()
        .append("line")
        .attr("class", "link"); 

    link.append("title")
        .text(function (d) { return d.type; });

    node = svg.selectAll(".node")
        .data(nodes)
        .enter()
        .append("g")
        .attr("class", "node")
        .on('dblclick', dblClicked)
        .on('click', clicked);
        //.call(d3.drag()
        //    .on("start", dragstarted)
        //    .on("drag", dragged)
    //);
    node.append("circle")
        .attr("r", d => d.nodeRadius)
        .style("fill", function (d, i) { if (d.isDirectNeighbor) { return colors(i); } return "#DDDDDD" });
    node.append("title")
        .text(d => d.name);

    // Text Attributes for nodes
    node.append("text")
        .attr("dx", d => d.nodeRadius)
        .attr("dy", ".35em")
        .attr("font-family", "sans-serif")
        .attr("font-size", "20px")
        .attr("font-weight", d => d.fontweight)
        .attr("fill", function (d, i) { if (d.isDirectNeighbor) { return "black"; } return "#DDDDDD" })
        .text(d => d.name);

    edgepaths = svg.selectAll(".edgepath")
        .data(links)
        .enter()
        .append('path')
        .attrs({
            'class': 'edgepath',
            'fill-opacity': 0,
            'stroke-opacity': 0,
            'id': function (d, i) { return 'edgepath' + i; },
            'marker-end': 'url(#arrowhead)'
        })
        .style("pointer-events", "none");


    simulation
        .nodes(nodes)
        .on("tick", ticked);
    simulation.force("link")
        .links(links);
    document.getElementById("entity-loading-indicator").style.display = "none";

}

function ticked() {
    node
        .attr("transform", function (d) { return "translate(" + d.x + ", " + d.y + ")"; });

    link
        .attr("x1", function (d) { return d.source.x; })
        .attr("y1", function (d) { return d.source.y; })
        .attr("x2", function (d) { return d.target.x; })
        .attr("y2", function (d) { return d.target.y; });

    edgepaths.attr('d', function (d) {
        return 'M ' + d.source.x + ' ' + d.source.y + ' L ' + d.target.x + ' ' + d.target.y;
    });

}

function dblClicked(d) {
    $('#q').val($('#q').val() + " " + d.name);
    $('#btn-search').click();
    Unload();
    document.getElementById("entity-loading-indicator").style.display = "block";
    //SearchEntities();
}

function clicked(d) {
    if (typeof d.clicked === 'undefined' || d.clicked === false) {
        d.clicked = true;
    } else {
        d.clicked = false;
    }
    setTimeout(function () {
        if (d.clicked) {
            d.clicked = false;
            updateOnClick(d);
        }
    }, 300, d);
}

function updateOnClick(d) {
    var facet = $('#facet-picker').val();
    Unload();
    document.getElementById("entity-loading-indicator").style.display = "block";
    GetGraph(d.name, facet);
}

//function dragstarted(d) {
//    if (!d3.event.active) {
//        simulation.alphaTarget(0.3).restart();
//    }
//    d.fx = d.x;
//    d.fy = d.y;
//}
//function dragged(d) {

//    // Check if movement beyond svg width/height and set to node
//    d.fx = Math.max(nodeRadius, Math.min(width - nodeRadius, d3.event.x));
//    d.fy = Math.max(nodeRadius, Math.min(height - nodeRadius, d3.event.y));
//}