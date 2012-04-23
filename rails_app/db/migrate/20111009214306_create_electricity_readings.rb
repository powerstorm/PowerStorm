class CreateElectricityReadings < ActiveRecord::Migration
  def self.up
    create_table :electricity_readings do |t|
      t.datetime :date_time
      t.integer :meter_id
      t.float :power

      t.timestamps
    end
  end

  def self.down
    drop_table :electricity_readings
  end
end
