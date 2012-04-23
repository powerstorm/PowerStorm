$j = jQuery.noConflict();
var a = location.href.split('/');
var abr = a[a.length - 1];
var tab_index = 0;
var update_types = ['Hour', 'Day', 'Month', 'Month'];
var tag_lookup = ['day', 'month', 'year', 'all_time'];
var usage_lookup = ['daily', 'monthly', 'yearly', 'monthly'];
var refresher;

function draw_chart() {
		if (tab_index != 0) {
			var from_date, to_date;
			to_date = (new Date()).sqlSafeStr();
			var i = tab_index - 1;
			switch(i) {
				case 0:
					from_date = (new Date()).changeTime('Hours', -24).sqlSafeStr();
					break;
				case 1:
					from_date = (new Date()).changeTime('Month', -1).sqlSafeStr();
					break;
				case 2:
					from_date = (new Date()).changeTime('FullYear', -1).sqlSafeStr();
					break;
				case 3:
					from_date = (new Date(2009, 8, 18)).sqlSafeStr();
					break;
			}
			
			jQuery.post('/ajax_update', {building: abr, type: update_types[tab_index - 1], from: from_date, to: to_date}, function(response_data) {
				var rd = eval(response_data).result;
				var data = new google.visualization.DataTable();
				data.addColumn('string', 'Hour');
				data.addColumn('number', 'KWh');
				data.addRows(rd.length + 1);
				
				/*data.setValue(0, 0, "12 AM");
				data.setValue(6, 0, "6 AM");
				data.setValue(12, 0, "12 PM");
				data.setValue(18, 0, "6 PM");*/
							
				for (var i = 0; i < rd.length + 1; ++i) {
					data.setValue(i, 1, rd[i]);
				}

				var chart = new google.visualization.LineChart(document.getElementById('advanced_chart'));
				chart.draw(data, {legend: 'none',
								  hAxis: {maxValue: 1, minValue: 24}
								 });
			});
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