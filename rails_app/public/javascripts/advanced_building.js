$j = jQuery.noConflict();
var a = location.href.split('/');
var abr = a[a.length - 1];
var tab_index = 0;
var update_types = ['Hour', 'Day', 'Month', 'Month'];
var tag_lookup = ['day', 'month', 'year', 'all_time'];
var usage_lookup = ['daily', 'monthly', 'yearly', 'monthly'];
var refresher;

function draw_chart() {
		var graph_type;
		if (tab_index != 0) {
			var from_date, to_date;
			to_date = (new Date()).sqlSafeStr();
			var i = tab_index - 1;
			switch(i) {
				case 0:
					from_date = (new Date()).changeTime('Hours', -24).sqlSafeStr();
					graph_type = "Hours";
					break;
				case 1:
					from_date = (new Date()).changeTime('Month', -1).sqlSafeStr();
					graph_type = "Month";
					break;
				case 2:
					from_date = (new Date()).changeTime('FullYear', -1).sqlSafeStr();
					graph_type = "FullYear";
					break;
				case 3:
					from_date = (new Date(2009, 8, 18)).sqlSafeStr();
					graph_type = "YTD";
					break;
			}
			
			jQuery.post('/ajax_update', {building: abr, type: update_types[tab_index - 1], from: from_date, to: to_date}, function(response_data) {
				var rd = eval(response_data).result;
				var data = new google.visualization.DataTable();
				data.addColumn('date', 'Date');
				data.addColumn('number', 'Power Usage (KWh)');
				       /* data.addRows([
				          [new Date(2008, 1 ,1), 30000, undefined, undefined, 40645, undefined, undefined],
				          [new Date(2008, 1 ,2), 14045, undefined, undefined, 20374, undefined, undefined],
				          [new Date(2008, 1 ,3), 55022, undefined, undefined, 50766, undefined, undefined],
				          [new Date(2008, 1 ,4), 75284, undefined, undefined, 14334, 'Out of Stock','Ran out of stock on pens at 4pm'],
				          [new Date(2008, 1 ,5), 41476, 'Bought Pens','Bought 200k pens', 66467, undefined, undefined],
				          [new Date(2008, 1 ,6), 33322, undefined, undefined, 39463, undefined, undefined]
				        ]);*/

				//Label the graph using the data points stored in rd (an array of power usage readings)
				label_graph(data, graph_type, rd, from_date, to_date);

				//Create the chart, place it in the advanced_chart div located on the building view
				var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('advanced_chart'));
				
				//Draw the chart using the data, we won't use annotations
				chart.draw(data, {displayAnnotations: false});						
			});
		}
	}
	
	//This function will label the different graph's X axis, as well as naming the Title of the graph
	function label_graph(data, graph_type, rd, from_date, to_date){
	
		//Since there could be many data points, we may have to find a percentage distance to label from
		//Find the length of rd to find how far labels should be placed.
		
		if(graph_type == "Hours"){
			//grab the hour of the beginning date
			//var beginning_hour = from_date.getHours();

			//For each data point, add it to the graph, separated by an hour
			//var i = 0;
			var new_from_date = Date.strToDate(from_date)
			for(i = 0; i < 24 ;i++){
				
				//data.addRow([(Date.strToDate(from_date)).changeTime('Hours', i), rd[i]]);
				data.addRow([new Date(new_from_date.getFullYear(), new_from_date.getMonth(), new_from_date.getDate(), new_from_date.getHours(), new_from_date.getMinutes(), new_from_date.getSeconds(), new_from_date.getMilliseconds()), rd[i]]);
				//add an hour each time through.
				new_from_date.changeTime('Hours', 1);
			}
			
		}else if(graph_type == "Month"){

		}else if(graph_type == "FullYear"){

		}else if(graph_type == "YTD"){

		}else{
			//Don't know how you got here
		}	
	}
	
	function reload_todays_usage() {
		if (tab_index != 0) {
			var i = tab_index - 1;
			
			$j.post('/ajax_update', {building: abr, type: 'other'/*, from: from_date, to: to_date*/}, function(response_data) {
				var d = eval(response_data);
				var l = tag_lookup[i].toLowerCase();
				var ll = usage_lookup[i];
				console.log(d);
				
				$j('#' + l + '_current_usage').html(d.current);
				$j('#' + l + '_usage').html(roundNumber(d[ll], 2));
				$j('#' + l + '_use_sqft').html(roundNumber(d[ll] / d.sqft, 4));
				$j('#' + l + '_use_occupant').html(roundNumber(d[ll] / d.occupants, 2));
				$j('#' + l + '_green_house').html(roundNumber(0.00068956 * d[ll], 2)); // emission factor: 6.8956 x 10^(-4) metric tons CO2 / kWh
			});
		}
	}

window.onload = function() {
	$j = jQuery.noConflict();
	$j( "#tabs" ).tabs({
		select: function (event, ui) {
			tab_index = ui.index;
			draw_chart();
			clearInterval(refresher);
			//refresher = setInterval(function() {
				reload_todays_usage();
			//}, 10000);
		}
	});
	$j( "#tabs" ).tabs({ selected: 1 });
	draw_chart();
}