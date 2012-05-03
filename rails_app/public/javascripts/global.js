function roundNumber(num, dec) {
	var result = Math.round(num*Math.pow(10,dec))/Math.pow(10,dec);
	return result;
}

Date.prototype.changeTime = function (incr, dif) {
	this['set'+incr](this['get'+incr]()+dif);
	return this;
}

//Takes a Date object, converts it to a string that is safe for insertion into a database
Date.prototype.sqlSafeStr = function (param) {
	var str = this.getFullYear() + '-'
		+ (this.getMonth() + 1) + '-'
		+ this.getDate() + ' '
		+ this.getHours() + ':'
		+ this.getMinutes() + ':'
		+ this.getSeconds();
	return str;
}

//Takes a string representation of the Date, and returns a Date object
Date.strToDate = function(date_string){
	//date_string looks like:  "2012-5-2 18:53:56"

	//Date object looks like:  "Date { Wed May 02 2012 18:53:56 GMT-0700 (PDT) }"
	var new_date = new Date();
	
	//Goal is to convert the date_string into the new_date and return it.
	
	//Replace all the spaces using colons
	date_string = date_string.replace(/\ /g, ":");
	
	//Replace all the dashes using colons
	date_string = date_string.replace(/\-/g, ":");
	
	//Lets split up the string by ":"
	var date_attributes = date_string.split(":");
	
	//Sets the year
	new_date.setFullYear(parseInt(date_attributes[0]));
	
	//Sets the month
	new_date.setMonth(parseInt(date_attributes[1]-1));
	
	//Sets the day of the month
	new_date.setDate(parseInt(date_attributes[2]));
	
	//Sets the hour
	new_date.setHours(parseInt(date_attributes[3]));
	
	//Sets minutes 
	new_date.setMinutes(parseInt(date_attributes[4]));
	
	//Sets seconds
	new_date.setSeconds(parseInt(date_attributes[5]));
	
	return new_date;	
}


/*
now = new Date();

console.log(now.sqlSafeStr());
console.log(now.changeTime('FullYear', -20).sqlSafeStr());
console.log(now.changeTime('Month', -20*12).sqlSafeStr());
console.log(now.changeTime('Date', -20*365).sqlSafeStr());
console.log("");

*/