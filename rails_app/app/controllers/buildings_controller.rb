ReadingsPerHour = 12
class BuildingsController < ApplicationController
  skip_before_filter :authorize, :only => [:index, :show, :ajax_update, :change_view_mode]
  @@TIME_OFFSET = 90.minutes
  
  # GET /buildings
  # GET /buildings.xml
  def index
	session[:view_mode] = "basic" if session[:view_mode].nil?
    @buildings = Building.all
    @logged_out = User.find_by_id(session[:user_id]).nil?

    respond_to do |format|
      format.html # index.html.erb
      format.xml  { render :xml => @buildings }
    end
  end

  # GET /buildings/1
  # GET /buildings/1.xml
  def show
	session[:view_mode] = "basic" if session[:view_mode].nil?
	params[:on_show_page] = true
    if params[:abbreviation]
      @building = Building.where(:abbreviation => params[:abbreviation]).first
    else
      @building = Building.find(params[:id])
    end

    respond_to do |format|
      format.js { render :json => @building }
      format.html # show.html.erb
      format.xml  { render :xml => @building }
    end
  end

  # GET /buildings/new
  # GET /buildings/new.xml
  def new
    @building = Building.new

    respond_to do |format|
      format.html # new.html.erb
      format.xml  { render :xml => @building }
    end
  end

  # GET /buildings/1/edit
  def edit
    @building = Building.find(params[:id])
  end

  # POST /buildings
  # POST /buildings.xml
  def create
    @building = Building.new(params[:building])

    respond_to do |format|
      if @building.save
        format.html { redirect_to(@building, :notice => 'Building was successfully created.') }
        format.xml  { render :xml => @building, :status => :created, :location => @building }
      else
        format.html { render :action => "new" }
        format.xml  { render :xml => @building.errors, :status => :unprocessable_entity }
      end
    end
  end

  # PUT /buildings/1
  # PUT /buildings/1.xml
  def update
    @building = Building.find(params[:id])

    respond_to do |format|
      if @building.update_attributes(params[:building])
        format.html { redirect_to(@building, :notice => 'Building was successfully updated.') }
        format.xml  { head :ok }
      else
        format.html { render :action => "edit" }
        format.xml  { render :xml => @building.errors, :status => :unprocessable_entity }
      end
    end
  end

  # DELETE /buildings/1
  # DELETE /buildings/1.xml
  def destroy
    @building = Building.find(params[:id])
    @building.destroy

    respond_to do |format|
      format.html { redirect_to(buildings_url) }
      format.xml  { head :ok }
    end
  end
  
  #This function calls the Routines in the MySQL database
  def send_chart info, period, building_id, from_date, to_date
	  queryResult = Building.connection.execute("CALL powerstorm_data.advancedChartBy#{period}(#{building_id}, '#{from_date}', '#{to_date}');")
	  #.to_a.transpose.first.collect { |i| (i * 100).to_i / 100.0 }
	  queryResult.each do |tuple|
		info[:power_usages].push tuple[0]
		info[:date_times].push tuple[1]
	  end
	  info
  end
  
  def send_response info
	respond_to do |format|
		  format.json { render :json => info }
	end
  end 
  
  def build_usage_hash rg_readings
	hash_readings = Hash.new
	for i in 0 .. rg_readings.length - 1 do 
		# sum up the readings for each record
		meter_readings = rg_readings[i]
		meter_readings.each  do |reading|
			if not hash_readings[reading.date_time].nil? then 
				hash_readings[reading.date_time] += reading.power
			#set the current time to the greatest time in the array
			else 
				hash_readings[reading.date_time] = reading.power
			end
		end
	end
	
	hash_readings
  end
  
  def get_top_usage building, top_n
	
		rgtop_readings = Array.new()
		building.meters.each do |meter|
			#get the 12 most recent readings for each meter and put the results into an array
			rgtop_readings.push(meter.electricity_readings.select("date_time,power").order(:date_time).reverse_order.limit(top_n));		
		end
		build_usage_hash rgtop_readings 
	end
  #get an hash of usage by datetime from a building for a given time window
  def get_usage building, time_start, time_end
	#for each meter on the building
	rgtop_readings = Array.new()
	@building.meters.each do |meter|
		#get the 12 most recent readings for each meter and put the results into an array
		rgtop_readings.push(meter.electricity_readings.select("date_time,power").where("? <= date_time <= ?",time_start,time_end));
	end
	#with our array we now add up the values by datetime into a hash
	build_usage_hash rgtop_readings
	#for each query get each record 
	#puts("rgtop_readings is #{rgtop_readings.inspect}")
	
end
	
  
  def basic_update params
  
  puts("!!!! WE ARE IN BASIC_UPDATE!")
  # draw_barchart() needs
# readings_time_interval
# weighted_current_kwh
# real_current_kwh
# feb_sum
  #This function gets the power usage at the more recent update and displays it. 
	@building = Building.where(:abbreviation => params[:building]).first

	#info = {:min => 0, :max => 0, :real_current_kwh => 0, :hourly => 0, :daily => 0, :monthly => 0, :yearly => 0, :sqft => 0, :occupants => 0, :readings_time_interval => 0, :feb_sum => 0, :weighted_current_kwh => 0}
	#info[:sqft] = @building.area
	#info[:occupants] = @building.capacity
	
	info = {:max => 0, :real_current_kwh => 0, :readings_time_interval => 0, :feb_sum => 0, :weighted_current_kwh => 0}
	
	#------------------
	# time variables
	#------------------
	# if the time must be offset (due to data delays), that is carried out here		
	current_reading_time = Time.now - @@TIME_OFFSET
	
	# !!! Make sure to avoid overwriting values here! They might be added together over the course of multiple
	#   iterations of the loop! Use +=, -=, etc only for info[] assignments
	weighting_added_fat = 0   # this variable declared outside loop in order to account for above
	first_loop = true         # flag to use so that certain operations in the loop below only happen once		#info[:real_current_kwh] += meter.electricity_readings.where("date_time <= ?", rounded_time.inspect).order(:date_time).reverse_order.first.power
	
	hash_readings = get_top_usage @building, ReadingsPerHour
	current_time = hash_readings.keys.max
	info[:max] = hash_readings.values.max
	puts("current_time is: #{current_time}")
	time_interval = (hash_readings.keys[0] - hash_readings.keys[1])/60
	# #puts("time_interval is: #{time_interval}") 
	info[:readings_time_interval] = time_interval
	info[:real_current_kwh] = hash_readings[hash_readings.keys.max]
	
	rounded_time = current_time - ( current_time.min % time_interval.to_i ).minutes
	rounded_time = rounded_time - rounded_time.sec.seconds
	# #sort sorts in descending order
	timesInOrder = hash_readings.keys.sort
	weight = 0.5
	for i in 0..ReadingsPerHour - 1
		info[:weighted_current_kwh] += 	(weight * hash_readings[timesInOrder[i]])
		weight = weight / 2
	end
	#info[:weighted_current_kwh] = info[:weighted_current_kwh] / weighting_added_fat
	@building.meters.each do |meter|
		feb_hour = ''
		if rounded_time.hour.to_i < 10
		  feb_hour = '0' + rounded_time.hour.inspect
		else
		  feb_hour = rounded_time.hour.inspect
		end
		feb_time = '2012-02-__ '  + feb_hour + ':__:__'
		#puts( 'feb time: ' + feb_time = '2012-02-__ '  + rounded_time.hour.inspect + ':__:__' )
		feb_readings = meter.electricity_readings.where("electricity_readings.date_time like ?", feb_time)
  
		puts( "feb time length: " + feb_readings.length.inspect)
		puts( "!!! INFO IS : #{info.inspect}")
		#sum all of the returned readings, divide values to account for number of 
		#days during this feb (29), the number of hours in a day (24), and the readings_per_hour
		for i in 0..feb_readings.length - 1
		  info[:feb_sum] += feb_readings[i].power / 29 / ReadingsPerHour
		end
	end
	
	send_response info
			
 
end
	
	def get_building abr
		Building.where(:abbreviation => abr).first
    end 
  def update_buildings 
	info = []
	buildings = Building.all
	buildings.each do |b|
		info << {:name => b.building_name, :abbreviation => b.abbreviation} 
	end
	send_response info
  end
  
  def update_today params
	info = {:current => 0, :usage => 0, :sqft => 0, :occupants => 0}
	@building = get_building(params[:building])
	info[:sqft] = @building.area
	info[:occupants] = @building.capacity
	current_reading_time = Time.now - @@TIME_OFFSET
	usage_hash = get_usage(@building, (current_reading_time - 1.day), current_reading_time)
	info[:usage] = usage_hash.values.sum
	info[:current] = usage_hash[usage_hash.keys.max]
	puts("info is: #{info.inspect}")
	send_response info 
  end
  def ajax_update
	 
	unless params[:building] == "all"
	
		case params[:type] 
			when "update" then basic_update params
			when "load" then update_buildings
			when "todays_usage" then update_today params
		else  
			@building = Building.where(:abbreviation => params[:building]).first
		    info = {:date_times => [], :power_usages => []}
			send_chart(info, params[:type], @building.id, params[:from], params[:to])
			send_response info
		end
			#ajax_basic_update params
			# @building = Building.where(:abbreviation => params[:building]).first

			
			
			# info[:sqft] = @building.area
			# info[:occupants] = @building.capacity
			
			# #------------------
			# # time variables
			# #------------------
			# # if the time must be offset (due to data delays), that is carried out here		
			# current_reading_time = Time.now - @@TIME_OFFSET
			
			# # !!! Make sure to avoid overwriting values here! They might be added together over the course of multiple
			# #   iterations of the loop! Use +=, -=, etc only for info[] assignments
			# weighting_added_fat = 0   # this variable declared outside loop in order to account for above
			# first_loop = true         # flag to use so that certain operations in the loop below only happen once
			
			# @building.meters.each do |meter|
			  # # get variable for top readings
				# top_readings = meter.electricity_readings.order(:date_time).reverse_order
			  
			  # # gets the time interval between readings in minutes
				# time_interval = ((top_readings.first.date_time - top_readings.second.date_time) / 60 ).to_i
				# info[:readings_time_interval] = time_interval
				
				# #puts("Time Interval: " + time_interval.inspect)
				
				# # get index of the least recent row with the same time_interval as the current readings up to 1300 readings.
				# # This value will be used to establish a recent range of max and min values used in charts on the site
				# index = 0
				# least_recent_date_time_with_current_time_interval = ''
				
				# for i in 0..meter.electricity_readings.length-1
				  # index = i
				  # #puts ( '!@!@!@ Index: ' + index.inspect + 'Num: ' + ((top_readings[i].date_time - top_readings[i + 1].date_time)/60).round.to_i.inspect)			    
			
				  # if ( ((top_readings[i].date_time - top_readings[i + 1].date_time)/60).round.to_i != time_interval) or i == 1300
						# least_recent_date_time_with_current_time_interval = meter.electricity_readings.order(:date_time).reverse_order[i].date_time
					# break
				  # end
				# end
				
				# #puts ("Index of least_4recent_time_interval: " + index.inspect )
				
				# # just get max and min during the current time_interval period
				# info[:min] += meter.electricity_readings.where("date_time > ?", least_recent_date_time_with_current_time_interval).order(:power).first.power
				# info[:max] += meter.electricity_readings.where("date_time > ?", least_recent_date_time_with_current_time_interval).order(:power).reverse_order.first.power
				
				# #puts( '!!! Max, Min Complete' )
				
				# # floor time to nearest time_interval minutes
				# rounded_time = current_reading_time - ( current_reading_time.min % time_interval.to_i ).minutes
				# rounded_time = rounded_time - rounded_time.sec.seconds
				# #THIS IS A SLOW PART AND IT HAPPENS 12 times? 
				# #puts("!!!!!!!!!!!!!!!! rounded_time - " + rounded_time.inspect)
				
				# ##puts(meter.electricity_readings.where(:date_time => rounded_time.inspect).order(:date_time).reverse_order.first.power.inspect)
				
				# info[:real_current_kwh] += meter.electricity_readings.where("date_time <= ?", rounded_time.inspect).order(:date_time).reverse_order.first.power
				
				# readings_per_hour = 60/time_interval
				
				# #puts( '!!! After real_current_kwh' )
				
				# # We return a weighted current electricity rating over readings from the past hour to account for some of the drastic spikes
				# # we see in energy usage due to the nature of electricity consumption. This is used to control the color_rating. When
				# # we do this weighting, the value is inflated. Thus, we must keep track of how much it is inflated and divide by that value at the
				# # end in order to normalize the readings to the real, non-weighted readings
				
			# if first_loop == true
				# for i in 0..readings_per_hour - 1
				  # weighting_added_fat += 0.5/readings_per_hour * (readings_per_hour - i)
				# end
				# first_loop = false
			# end
			  
			  # # get index of the offset current time row
			  # offset_index = 0
			# while meter.electricity_readings.order(:date_time).reverse_order[offset_index].date_time.inspect[17..21] != rounded_time.inspect[11..15]
			   # # #puts("Meter: " + meter.electricity_readings.order(:date_time).reverse_order[offset_index].date_time.inspect[0..21])
				# ##puts("Rounded_time: " + rounded_time.inspect[0..15])
				# offset_index += 1
			# end
			  
			# for i in 0..readings_per_hour - 1
			  # info[:weighted_current_kwh] += (0.5/readings_per_hour * (readings_per_hour - i))*(top_readings[offset_index + i].power)
			# end
				
		  # #puts( '!!! Weighted Current kWh set.' )
				
				
			# # Currently, readings from the month of February during the same hour as the current_time hour are used as the baseline
			# # for the choosing the current color_rating
			# # Currently this is done lazily, just taking the rows who have the same hour digit as the current time.
			# feb_hour = ''
			# if rounded_time.hour.to_i < 10
			  # feb_hour = '0' + rounded_time.hour.inspect
			# else
			  # feb_hour = rounded_time.hour.inspect
			# end
			# feb_time = '2012-02-__ '  + feb_hour + ':__:__'
			# ##puts( 'Feb time: ' + feb_time = '2012-02-__ '  + rounded_time.hour.inspect + ':__:__' )
			# feb_readings = meter.electricity_readings.where("electricity_readings.date_time LIKE ?", feb_time)
		  
		  # #puts( "Feb time Length: " + feb_readings.length.inspect)
			
			# # sum all of the returned readings, divide values to account for number of 
			# # days during this feb (29), the number of hours in a day (24), and the readings_per_hour
			# for i in 0..feb_readings.length - 1
			  # info[:feb_sum] += feb_readings[i].power / 29 / readings_per_hour
			# end
			
			# # divide by the number of days during this feb (29), the number of hours in a day (240, and the readings_per_hour
			# ##puts( "Before divide: " + info[:feb_sum].inspect )
			# #info[:feb_sum] = info[:feb_sum] / 29 / readings_per_hour
			# ##puts( "Before divide: " + info[:feb_sum].inspect )
			
			# ##puts ("01010101010101" + feb_readings.inspect )
			
			# ##puts("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
			
			# # old method (just takes the top reading, not the time from @@TIME_OFFSET ago)
			# #info[:current] += (meter.electricity_readings.order(:date_time).reverse_order.first.power * 1000).to_i / 1000.0
			

		# end
		
		# #puts ( "Added Fat: " + weighting_added_fat.inspect + '  Weighted Current: ' + info[:weighted_current_kwh].inspect)
		# info[:weighted_current_kwh] = info[:weighted_current_kwh] / weighting_added_fat
		# #puts ( '  Weighted Current: ' + info[:weighted_current_kwh].inspect)
		
		# if ["Day", "Month", "Year", "Hour", "Week"].include?(params[:type]) 
			# #Date.parse(params[:from])
			# #info[:result] = Building.connection.execute("CALL chartByHour(11, '2009-10-25 18:44:11', '2009-10-26 18:44:11');").to_a.transpose.first;
			# #Building.find(11).connection.execute("CALL chartByHour(11, '2009-10-25 18:44:11', '2009-10-26 18:44:11');").to_a.transpose.first
			
			# ##puts(" $$$$$$$$ #{period}(#{building_id}, '#{from_date}', '#{to_date}');")
			# #puts("$$$" + params.inspect)

		
			# #send_chart(info, params[:type], @building.id, '2012-4-27 13:51:16', '2012-4-28 13:51:16')

		# else
			# #puts("||||||||||||||||         CALL powerstorm_data.getSumsForBuilding(#{@building.id},'#{(Time.now - @@TIME_OFFSET).to_s(:db)}')")
			# arr = Building.connection.execute("CALL powerstorm_data.getSumsForBuilding(#{@building.id},'#{(Time.now - @@TIME_OFFSET).to_s(:db)}')").first;
			# info[:hourly] = arr[3]
			# info[:daily] = arr[2]
			# info[:monthly] = arr[1]
			# info[:yearly] = arr[0]
			# #puts(">>>>>>>>>>>>>" + info.inspect)
		# end
	
		# # else
			# # info = []
			# # buildings = Building.all
			# # buildings.each do |b|
				# # info << {:name => b.building_name, :abbreviation => b.abbreviation} 
			# # end
		
	
		# respond_to do |format|
		  # format.json { render :json => info }
		# end
	# end
  end
  end
  def change_view_mode
	session[:view_mode] = params[:view_mode]
    respond_to do |format|
	  format.html { redirect_to(request.referer) }
	end
  end  
end