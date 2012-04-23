class Meter < ActiveRecord::Base
  belongs_to :building
  has_many :electricity_readings
end
