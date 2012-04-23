// Function to get all elements of a specific class
document.getElementsByClassName = function(cl) {
	var retnode = [];
	var myclass = new RegExp('\\b'+cl+'\\b');
	var elem = this.getElementsByTagName('*');
	for (var i = 0; i < elem.length; i++) {
	var classes = elem[i].className;
	if (myclass.test(classes)) retnode.push(elem[i]);
	}
	return retnode;
};

window.onresize = function(event) {
    /* The following blocks were attempts to get the raphael paper to scale, it is not currently working
    //resizePaper();
    //document.getElementById("paper").innerHTML = "";
    //R.clear();
    //draw_svg_map(map_width(), map_height( map_width() ));
    */

    /* Having issues with resizing raphael, so image won't be resized right now
    // resize image
    document.getElementById("map_image").width = map_width();
    document.getElementById("map_image").height = map_height( map_width() );
    */
    // move the paper div so that the svg lines up with the map image
    align_divs_under_nav_bar();

    // store current page width/height
    original_w = window.innerWidth;
    original_h = window.innerHeight;
};

window.onload = function () {
    last_sizeX = map_width();
    original_w = window.innerWidth;
    original_h = window.innerHeight;
    draw_svg_map(map_width(), map_height( map_width() ));
    resizePaper();
    align_divs_under_nav_bar();
};

align_divs_under_nav_bar = function() {
    // realign the raphael paper div over the image
    var head_height = document.getElementById("head").offsetHeight;
    head_height = head_height + 5;
    var paper_height = document.getElementById("paper").offsetHeight;
    var paper_div = document.getElementById("paper");
    //paper_div.style.top = (head_height - paper_height + 42) +'px';  // for use with scale raphael
    paper_div.style.top = head_height + 'px';
    //paper_div.style.left = -10;   doesn't work
    
    // realign the building information divs to below the header bar
	/*
    var elements = document.getElementsByClassName("buildings");
    var count = 0;
    while (count < elements.size() ) {
	    var current = elements[count];
	    current.style.top = head_height + 'px';
	    count++;
    }
	*/
};

resizePaper = function(){
   var w = 0, h = 0;
   if (window.innerWidth){
      w = window.innerWidth;
      h = window.innerHeight;
   }else if(document.documentElement &&
           (document.documentElement.clientWidth || 
            document.documentElement.clientHeight)) { 
            w = document.documentElement.clientWidth;
            h = document.documentElement.clientHeight;
   }
   //R.changeSize(w, h, true, false);
   //R.scaleAll( last_sizeX );
}

scale_percentage = function ( last_w ) {
    var scale = map_width() / last_w;
    return scale;
}

// calculate the width as 70% of window size
map_width = function() {
    var width = .70 * window.innerWidth;
    return width;
}

// Calculate the height as a function of the width to maintain aspect ratio (w = 746, h = 493)
map_height = function(width) {
    var height = width * 493 / 746;
    return height;
}


draw_svg_map = function(sizeX, sizeY){
    //var new_scale = 1;
    //if ( sizeX != last_sizeX ) { new_scale = scale_percentage( last_sizeX ); }

    R = new Raphael("paper", 746, 493);		//change dimensions to sizeX, sizeY for dynamic scaling (doesn't yet work)
	R.image("/images/map_transparent.png",-5,3,746,488);		// offset a bit for cool 3d look
    var attr = {
        fill: "#333",
        stroke: "#666",
        "stroke-width": 1,
        "stroke-linejoin": "round"
    };
    var whitworth = {};
    whitworth.acs = R.path("M107.333,106.013L157.333,144.68L160.5,140.847L164,144.68L168.333,137.847L166.167,135.346L178.667,119.68L182.167,122.346L192.667,115.847L203.667,103.846L217.167,113.18L230.834,99.346L195.667,72.013L184.001,86.014L181.5,84.18L175,91.513L178,94.513L172.5,101.846L134.167,73.513Z").attr(attr);
    whitworth.wmh = R.path("M292.333,101.347L292.333,114.181L321.832,114.181L321.832,104.847L332,105.014L332,123.681L284.166,123.681L284.166,101.347Z").attr(attr);

    whitworth.lcv = R.path("M349.666,111.681L382.331,111.681L386.332,107.681L396.998,117.848L388.498,128.514L382.498,123.681L349.666,123.681Z").attr(attr);

    whitworth.shh = R.path("M385.832,149.681L413.666,145.181L415.999,157.515L415.999,159.014L410.165,159.014L410.666,168.014L396.166,168.014L396.331,161.182L388.332,163.015Z").attr(attr);

    whitworth.fas = R.path("M421.165,105.514L423.832,99.348L441.997,106.348L443.165,103.681L460.164,110.014L465.497,99.348L492.664,108.514L486.664,122.848L482.497,120.515L479.332,126.181Z	").attr(attr);

    whitworth.bjh = R.path("M565.332,131.682L579.664,131.682L579.664,137.682L585.165,137.682L585.165,131.682L597.664,131.682L597.664,135.182L614.664,135.182L613.498,164.849L598.998,164.849L599.665,146.682L595.665,146.682L595.665,151.182L583.665,150.848L583.998,145.848L578.165,145.848L577.831,162.682L563.832,162.015Z").attr(attr);

    whitworth.eah = R.path("M672.329,148.265L678.329,141.349L697.33,155.683L699.495,153.516L706.996,159.516L704.996,163.683L713.829,187.349L703.662,191.516L695.162,167.849Z").attr(attr);

    whitworth.duh = R.path("M703.329,206.849L695.329,251.016L710.828,253.683L719.163,209.35Z").attr(attr);

    whitworth.boh = R.path("M660.663,270.349L670.663,270.349L670.663,278.849L671.662,278.849L682.33,268.849L691.163,278.016L667.163,301.349L658.163,292.682L668.662,281.683L668.162,280.183L659.33,279.516Z").attr(attr);

    whitworth.hah = R.path("M675.666,349.848L689.834,349.848L689.834,366.848L687.667,366.848L688.001,368.848L705.5,368.848L705.5,356.682L713.334,357.015L713.334,381.848L679.667,381.848L679.834,367.515L675.666,366.848Z").attr(attr);



    whitworth.prh = R.path("M556.001,446.682L564.169,446.682L564.169,453.015L566.335,453.015L563.502,459.515L561.167,459.515L561.167,463.515L554.835,463.515L554.835,451.182L553.169,450.848L553.169,448.182L554.835,448.182L554.835,446.682Z").attr(attr);

    whitworth.cau = R.path("M388.503,391.514L408.67,369.014L405.836,366.347L407.836,363.181L396.336,354.014L393.836,355.847L390.169,352.514L369.836,375.265L373.504,378.68L371.17,381.514L382.004,390.514Z").attr(attr);

    whitworth.mbl = R.path("M311.17,374.514L329.504,374.514L329.504,377.514L332.336,377.514L334.67,376.014L335.504,374.514L346.67,374.514L346.67,387.681L332.504,387.681L328.92,395.681L318.503,395.681L318.503,397.181L312.336,396.348L312.004,385.848L315.504,385.848L315.504,383.181L311.17,382.514Z").attr(attr);

    whitworth.dxh = R.path("M364.67,328.514L372,321.348L375.004,324.931L376.67,322.848L396.836,340.848L389.254,348.514L364.67,330.514Z").attr(attr);

    whitworth.meh = R.path("M419.836,369.514L438.67,384.681L445.669,376.181L426.837,360.848Z").attr(attr);


    whitworth.wah = R.path("M325.67,308.18L328.92,304.681L321.004,299.514L326.836,292.18L343.504,306.014L346.67,302.848L365.67,317.514L360.336,324.847L345.004,312.014L340.837,318.014L341.67,319.014L324.17,341.68L316.504,334.93L332.004,315.68L324.336,309.014Z	").attr(attr);

    whitworth.hub = R.path("M509.208,301.349L537.209,280.68L542.583,286.43L552.083,286.805L552.083,294.681L571.834,294.681L571.834,280.68L587.584,280.68L587.584,294.805L576.584,294.681L576.459,315.431L569.459,315.431L556.584,325.556L556.709,333.43L551.209,333.43L550.833,327.931L540.583,317.18L530.709,324.68L522.334,316.431L522.459,309.181L515.833,309.305Z").attr(attr);

    whitworth.heh = R.path("M560.666,208.347L567.501,208.347L565.834,242.848L557.167,242.848L558.334,230.266L555.666,230.266L555.666,223.348L559.5,223.348Z").attr(attr);


    whitworth.arh = R.path("M520.001,249.514L549.501,249.514L549.501,242.848L554.667,242.848L554.667,250.847L573.333,250.847L573.333,251.847L584.166,251.847L584.166,263.347L563.501,263.347L563,266.514L548.396,265.514L548.396,260.847L520.001,259.68Z").attr(attr);

    whitworth.sth = R.path("M591.167,191.516L620.333,214.014L629.001,204.348L599.334,181.18Z").attr(attr);


    whitworth.hcc = R.path("M464.521,228.305L485.959,228.305L484.646,270.806L462.833,270.806L462.833,258.618L450.146,258.556L450.146,263.493L446.834,262.993L446.834,267.368L433.709,267.118L433.709,263.493L430.833,263.368L430.833,241.805L464.459,241.868Z").attr(attr);

    whitworth.jsc = R.path("M522.918,183.514L540.25,183.514L540.25,195.764L534.667,195.764L534.585,220.598L536.251,220.598L536.335,228.847L529.668,228.847L529.585,223.764L501.667,223.68L501.667,222.264L497.417,222.264L497.417,210.847L501.418,210.68L501.418,208.347L523.25,208.347Z").attr(attr);

    whitworth.rsh = R.path("M446.335,161.597L493.585,161.597L493.585,179.347L488.585,179.347L488.585,183.514L457.584,183.514L457.584,186.514L446.335,186.514Z").attr(attr);

    whitworth.lic = R.path("M381.999,218.013L387.75,211.346L412.583,230.263L406.916,237.013L397.291,230.096L394.665,232.429L388.499,227.596L391.499,224.846Z").attr(attr);

    whitworth.whh = R.path("M346.332,183.68L350.249,179.596L353.582,182.68L358.999,175.763L375.332,187.846L370.749,193.679L375.915,199.179L368.832,206.929L363.499,201.929L360.332,205.179L353.416,200.429L355.832,198.43L352.998,195.763L350.832,196.679L345.416,191.929L349.916,187.013Z").attr(attr);

    whitworth.mmh = R.path("M274.958,197.243L282.333,187.681L290.583,194.368L295.521,187.681L304.708,193.93L300.771,198.618L302.771,200.306L300.333,203.555L302.896,206.368L296.646,213.805L294.646,212.243L291.896,214.868L294.708,217.43L289.833,222.993L276.896,213.931L284.896,205.336Z").attr(attr);

    whitworth.sgm = R.path("M299.584,265.931L314.668,244.597L331,256.931L325,265.264L322.75,264.431L312,275.431Z").attr(attr);

    whitworth.bah = R.path("M273.751,233.098L294.168,247.348L288.501,254.598L267.501,239.598Z").attr(attr);

    whitworth.pcs = R.path("M692.501,84.097L710.417,78.431L714.333,92.014L695.666,98.431Z").attr(attr);

    whitworth.thv = R.path("M630.917,167.097L648.251,161.43L651.583,168.43L657.417,160.18L672.75,169.596L666.166,178.263L672.329,183.93L663.584,197.597L654.083,192.347L661.001,178.889L663.584,177.263L653.334,170.43L634.833,178.096Z").attr(attr);

    whitworth.ggy = R.path("M204.833,225.305L221.083,238.681L244.584,212.681L228.458,198.93Z").attr(attr);

    whitworth.wpc = R.path("M520.75,350.764L534.418,353.514L533.918,358.014L551.334,361.18L550.251,369.847L552.75,370.681L551.834,373.93L555.167,375.347L553.833,378.097L553.251,378.931L544.001,376.431L541.333,381.597L543.251,382.514L544.751,379.264L552.668,380.931L549.083,401.681L535,399.18L536.917,391.514L518.416,388.514L520.666,379.014L531.75,381.014L531.917,378.68L539.084,379.93L540.25,370.681L531.666,369.847L530.5,377.097L513.999,374.431L513.999,368.514L510.999,368.098L513.583,355.014L517.25,354.681L517.584,352.014L520.25,352.431Z	").attr(attr);


    whitworth.hih = R.path("M343.667,443.167L358.667,442.334L358.667,453.501L343.667,453.501Z").attr(attr);

    whitworth.mkh = R.path("M428.667,418.167L440,418.167L440.833,428.334L428.667,428.334Z").attr(attr);

    whitworth.alh = R.path("M300.833,413.834L310.5,414.501L310.5,424.834L300.833,424.834Z").attr(attr);

    whitworth.cos = R.path("M104.375,416.625L136.625,417.625L136.625,430.875L104.375,430.875Z").attr(attr);

    //console.log(whitworth["hub"]);
    //whitworth["hub"].scale( new_scale, new_scale );
    //whitworth["hub"].scale( .1, .1 );


    var current = null;
    for (var state in whitworth) {
	//console.log(state);
        whitworth[state].color = Raphael.getColor();
	//console.log(whitworth[state]);
        // scale the path to current window size
        //whitworth[state].scale( new_scale, new_scale );
	//whitworth[state].scale( .1, .1 );
        (function (st, state) {
            st[0].onclick = function() {
              location.href = "/abr/" + String(state);
            };
            st[0].style.cursor = "pointer";
            st[0].onmouseover = function () {
                current && whitworth[current].animate({
                    fill: "#333",
                    stroke: "#666"
                }, 500) && (document.getElementById(current).style.display = "");
                st.animate({
                    fill: st.color,
                    stroke: "#ccc"
                }, 500);
                st.toFront();
                R.safari();
                document.getElementById(state).style.display = "block";
                current = state;
            };
            st[0].onmouseout = function () {
                st.animate({
                    fill: "#333",
                    stroke: "#666"
                }, 500);
                st.toFront();
                R.safari();
            };
            if (state == "nsw") {
                st[0].onmouseover();
            }
        })(whitworth[state], state);
    }

    // save last dimensions
    last_sizeX = sizeX;
    last_sizeY = sizeY;
};
