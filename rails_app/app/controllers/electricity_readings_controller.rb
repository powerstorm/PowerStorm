class ElectricityReadingsController < ApplicationController
  # GET /electricity_readings
  # GET /electricity_readings.xml
  def index
    @electricity_readings = ElectricityReading.all.reverse #.paginate(params[:page])

    respond_to do |format|
      format.html # index.html.erb
      format.xml  { render :xml => @electricity_readings }
    end
  end

  # GET /electricity_readings/1
  # GET /electricity_readings/1.xml
  def show
    @electricity_reading = ElectricityReading.find(params[:id])

    respond_to do |format|
      format.html # show.html.erb
      format.xml  { render :xml => @electricity_reading }
    end
  end

  # GET /electricity_readings/new
  # GET /electricity_readings/new.xml
  def new
    @electricity_reading = ElectricityReading.new

    respond_to do |format|
      format.html # new.html.erb
      format.xml  { render :xml => @electricity_reading }
    end
  end

  # GET /electricity_readings/1/edit
  def edit
    @electricity_reading = ElectricityReading.find(params[:id])
  end

  # POST /electricity_readings
  # POST /electricity_readings.xml
  def create
    @electricity_reading = ElectricityReading.new(params[:electricity_reading])

    respond_to do |format|
      if @electricity_reading.save
        format.html { redirect_to(@electricity_reading, :notice => 'Electricity reading was successfully created.') }
        format.xml  { render :xml => @electricity_reading, :status => :created, :location => @electricity_reading }
      else
        format.html { render :action => "new" }
        format.xml  { render :xml => @electricity_reading.errors, :status => :unprocessable_entity }
      end
    end
  end

  # PUT /electricity_readings/1
  # PUT /electricity_readings/1.xml
  def update
    @electricity_reading = ElectricityReading.find(params[:id])

    respond_to do |format|
      if @electricity_reading.update_attributes(params[:electricity_reading])
        format.html { redirect_to(@electricity_reading, :notice => 'Electricity reading was successfully updated.') }
        format.xml  { head :ok }
      else
        format.html { render :action => "edit" }
        format.xml  { render :xml => @electricity_reading.errors, :status => :unprocessable_entity }
      end
    end
  end

  # DELETE /electricity_readings/1
  # DELETE /electricity_readings/1.xml
  def destroy
    @electricity_reading = ElectricityReading.find(params[:id])
    @electricity_reading.destroy

    respond_to do |format|
      format.html { redirect_to(electricity_readings_url) }
      format.xml  { head :ok }
    end
  end
end
