class ElectricityReading < ActiveRecord::Base
  belongs_to :meter
  # TODO build up dependencies & stoff
end
