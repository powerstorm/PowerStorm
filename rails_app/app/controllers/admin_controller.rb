class AdminController < ApplicationController
  def index
    redirect_to buildings_url, :notice => "Logged In"
  end

end
