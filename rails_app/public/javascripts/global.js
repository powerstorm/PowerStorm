function roundNumber(num, dec) {
	var result = Math.round(num*Math.pow(10,dec))/Math.pow(10,dec);
	return result;
}

Date.prototype.changeTime = function (incr, dif) {
	this['set'+incr](this['get'+incr]()+dif);
	return this;
}

Date.prototype.sqlSafeStr = function (param) {
	this.changeTime('Date', -365);
	var str = this.getFullYear() + '-'
		+ (this.getMonth() + 1) + '-'
		+ this.getDate() + ' '
		+ this.getHours() + ':'
		+ this.getMinutes() + ':'
		+ this.getSeconds();
	return str;
}

/*
now = new Date();

console.log(now.sqlSafeStr());
console.log(now.changeTime('FullYear', -20).sqlSafeStr());
console.log(now.changeTime('Month', -20*12).sqlSafeStr());
console.log(now.changeTime('Date', -20*365).sqlSafeStr());
console.log("");

*/